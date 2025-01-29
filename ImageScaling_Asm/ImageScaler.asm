; ==============================
; Sekcja danych
; ==============================
.DATA
resizedBitmap QWORD ? ; Bufor dla nowego obrazu (opcjonalnie)

; ==============================
; Deklaracje procedur zewnêtrznych
; ==============================
EXTERN VirtualAlloc:PROC

; ==============================
; Sekcja kodu
; ==============================
.CODE

PUBLIC ScaleImageAsm

; -----------------------------------------------------------
; Funkcja: ScaleImageAsm
; -----------------------------------------------------------
; Argumenty (Microsoft x64):
;   RCX - bitmapPhoto       (wskaŸnik do oryginalnego obrazu)
;   RDX - originalWidth     (szerokoœæ oryginalna, int)
;   R8  - originalHeight    (wysokoœæ oryginalna, int)
;   R9  - newWidth          (nowa szerokoœæ, int)
;   (5-ty argument)         (newHeight, int) - na stosie
;
; Zak³adamy, ¿e prototyp w C++ to:
;   extern "C" unsigned char* ScaleImageAsm(
;       unsigned char* bitmapPhoto,
;       int originalWidth,
;       int originalHeight,
;       int newWidth,
;       int newHeight
;   );
;
; Zwraca: 
;   - wskaŸnik do nowego obrazu 
;   - NULL (0), jeœli wyst¹pi³ b³¹d
; -----------------------------------------------------------

ScaleImageAsm PROC
    ; -----------------------------------------------------
    ; Prolog
    ; -----------------------------------------------------
    push    rbp
    mov     rbp, rsp
    sub     rsp, 64            ; rezerwujemy miejsce na zmienne lokalne

    ; -----------------------------------------------------
    ; Zapisujemy 4 g³ówne argumenty do pamiêci lokalnej
    ; -----------------------------------------------------
    mov     [rsp +  0], rcx    ; [rsp +  0] = bitmapPhoto
    mov     [rsp +  8], rdx    ; [rsp +  8] = originalWidth
    mov     [rsp + 16], r8     ; [rsp + 16] = originalHeight
    mov     [rsp + 24], r9     ; [rsp + 24] = newWidth

    ; -----------------------------------------------------
    ; Wczytanie 5. argumentu (newHeight, int)
    ; UWAGA: W Windows x64 kolejne argumenty (po 4.) trafiaj¹ na stos.
    ;        Dlatego musimy pobraæ *tylko* 32 bity z w³aœciwej pozycji.
    ;        Je¿eli w C++ mamy 'int newHeight', to musimy zrobiæ mov eax, [rbp+?].
    ; -----------------------------------------------------

    ; Najczêœciej 5-ty argument (typu 32-bit int) jest w [rbp + 48],
    ; ale to zale¿y od uk³adu stosu w czasie wywo³ania.
    ; W praktyce, przy "push rbp / mov rbp, rsp", 
    ;   [rbp +  0] = poprzedni RBP
    ;   [rbp +  8] = adres powrotny
    ;   [rbp + 16..] = home space/rejestry
    ;   itd.
    ;
    ; Jeœli Twój kompilator/œrodowisko potwierdza, ¿e 5-ty argument
    ; jest pod [rbp + 48], wczytujemy go tak:
    ;

    mov     eax, DWORD PTR [rbp + 48] ; za³aduj 32 bity newHeight do EAX
    mov     [rsp + 32], rax           ; zapisz w [rsp+32] (jako int, z wyzerowan¹ gór¹ RAX)

    ; ------------------------------
    ; Sprawdzenie warunków b³êdnych (NULL, <= 0, itp.)
    ; ------------------------------

    ; 1. bitmapPhoto (64-bit wskaŸnik), sprawdzamy != NULL
    mov   rax, [rsp +  0]    ; wczytujemy wskaŸnik
    test  rax, rax           ; sprawdzamy, czy jest równy 0
    jz    error              ; jeœli 0 -> b³¹d

    ; 2. originalWidth (32-bit int), sprawdzamy > 0
    mov   eax, [rsp +  8]    ; wczytujemy 32 bity do EAX
    test  eax, eax           ; sprawdzamy znak i zero
    jle   error              ; jeœli <= 0 -> b³¹d

    ; 3. originalHeight (32-bit int), sprawdzamy > 0
    mov   eax, [rsp + 16]
    test  eax, eax
    jle   error

    ; 4. newWidth (32-bit int), sprawdzamy > 0
    mov   eax, [rsp + 24]
    test  eax, eax
    jle   error

    ; 5. newHeight (32-bit int), sprawdzamy > 0
    mov   eax, [rsp + 32]
    test  eax, eax
    jle   error

    ; -----------------------------------------------------
    ; newRowSize = (newWidth * 3 + 3) & ~3
    ; -----------------------------------------------------
    mov     rax, [rsp + 24]    ; RAX = newWidth
    imul    rax, 3             ; RAX = newWidth * 3
    add     rax, 3
    and     rax, -4            ; wyrównanie w dó³ (do wielokrotnoœci 4)
    mov     [rsp + 40], rax    ; newRowSize

    ; -----------------------------------------------------
    ; newPixelArraySize = newRowSize * newHeight
    ; -----------------------------------------------------
    mov     rbx, rax           ; rbx = newRowSize
    mov     rax, [rsp + 32]    ; rax = newHeight
    imul    rax, rbx           ; rax = newRowSize * newHeight
    test    rax, rax
    jle     error

    mov     [rsp + 48], rax    ; newPixelArraySize

    ; -----------------------------------------------------
    ; Wywo³anie VirtualAlloc (alokacja pamiêci)
    ; W MS x64 parametry s¹ w rejestrach: rcx, rdx, r8, r9
    ;
    ; LPVOID VirtualAlloc(
    ;     LPVOID lpAddress,    // rcx
    ;     SIZE_T dwSize,       // rdx
    ;     DWORD  flAllocationType, // r8
    ;     DWORD  flProtect     // r9
    ; );
    ; -----------------------------------------------------
    sub     rsp, 32            ; shadow space (zalecane)
    mov     rcx, 0             ; lpAddress = NULL
    mov     rdx, rax           ; dwSize = newPixelArraySize
    mov     r8,  4096          ; MEM_COMMIT
    mov     r9,  64            ; PAGE_READWRITE
    call    VirtualAlloc
    add     rsp, 32

    test    rax, rax
    jz      error

    ; Zapisujemy wskaŸnik do zaalokowanej pamiêci
    mov     [rsp + 56], rax    ; resizedBitmap

    ; -----------------------------------------------------
    ; Skalowanie metod¹ najbli¿szego s¹siada
    ; -----------------------------------------------------
    xor     rcx, rcx           ; y = 0

outer_loop:
    cmp     rcx, [rsp + 32]    ; y < newHeight ?
    jge     end_outer_loop

    xor     rdx, rdx           ; x = 0

inner_loop:
    cmp     rdx, [rsp + 24]    ; x < newWidth ?
    jge     end_inner_loop

    ; ---------------------------
    ; srcX = (x * originalWidth) / newWidth
    ; ---------------------------
    mov     rax, rdx
    imul    rax, [rsp +  8]    ; x * originalWidth
    cqo
    idiv    qword ptr [rsp + 24]  ; / newWidth
    mov     rsi, rax           ; srcX

    ; ---------------------------
    ; srcY = (y * originalHeight) / newHeight
    ; ---------------------------
    mov     rax, rcx
    imul    rax, [rsp + 16]    ; y * originalHeight
    cqo
    idiv    qword ptr [rsp + 32]  ; / newHeight
    mov     rdi, rax           ; srcY

    ; -------------------------------------------------
    ; WskaŸnik Ÿród³owy: 
    ;   sourcePtr = bitmapPhoto + ((srcY * originalWidth) + srcX) * 3
    ; -------------------------------------------------
    mov     rax, [rsp +  0]    ; base = bitmapPhoto

    mov     r8, rdi            ; r8 = srcY
    imul    r8, [rsp +  8]     ; r8 *= originalWidth
    imul    r8, 3
    add     rax, r8            ; rax -> pocz¹tek wiersza

    mov     r8, rsi            ; r8 = srcX
    imul    r8, 3
    add     rax, r8            ; rax -> (srcX) w tym wierszu

    ; Zachowujemy wskaŸnik Ÿród³a w r10
    mov     r10, rax

    ; -------------------------------------------------
    ; WskaŸnik docelowy:
    ;   destPtr = resizedBitmap + (y * newRowSize) + (x * 3)
    ; -------------------------------------------------
    mov     rbx, [rsp + 56]    ; rbx = resizedBitmap

    mov     r8, [rsp + 40]     ; r8 = newRowSize
    imul    r8, rcx            ; y * newRowSize
    add     rbx, r8            ; pocz¹tek wiersza docelowego

    mov     r8, rdx            ; x
    imul    r8, 3
    add     rbx, r8            ; offset w wierszu

    ; -------------------------------------------------
    ; Kopiowanie 3 bajtów: B, G, R
    ; -------------------------------------------------
    movzx   rsi, byte ptr [r10]        ; B
    mov     [rbx], sil
    movzx   rsi, byte ptr [r10 + 1]    ; G
    mov     [rbx + 1], sil
    movzx   rsi, byte ptr [r10 + 2]    ; R
    mov     [rbx + 2], sil

    ; x++
    inc     rdx
    jmp     inner_loop

end_inner_loop:
    ; y++
    inc     rcx
    jmp     outer_loop

end_outer_loop:
    ; -----------------------------------------------------
    ; Zwracamy wskaŸnik do nowego obrazu w RAX
    ; -----------------------------------------------------
    mov     rax, [rsp + 56]

    ; -----------------------------------------------------
    ; Epilog
    ; -----------------------------------------------------
    mov     rsp, rbp
    pop     rbp
    ret

; -----------------------------------------------------
; Obs³uga b³êdów:
; -----------------------------------------------------
error:
    xor     rax, rax   ; RAX = 0 (NULL)
    mov     rsp, rbp
    pop     rbp
    ret

ScaleImageAsm ENDP
END
