; ==============================
; Sekcja danych
; ==============================
.DATA
resizedBitmap QWORD ? ; Bufor dla nowego obrazu (opcjonalnie)

; ==============================
; Deklaracje procedur zewn�trznych
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
;   RCX - bitmapPhoto       (wska�nik do oryginalnego obrazu)
;   RDX - originalWidth     (szeroko�� oryginalna, int)
;   R8  - originalHeight    (wysoko�� oryginalna, int)
;   R9  - newWidth          (nowa szeroko��, int)
;   (5-ty argument)         (newHeight, int) - na stosie
;
; Zak�adamy, �e prototyp w C++ to:
;   extern "C" unsigned char* ScaleImageAsm(
;       unsigned char* bitmapPhoto,
;       int originalWidth,
;       int originalHeight,
;       int newWidth,
;       int newHeight
;   );
;
; Zwraca: 
;   - wska�nik do nowego obrazu 
;   - NULL (0), je�li wyst�pi� b��d
; -----------------------------------------------------------

ScaleImageAsm PROC
    ; -----------------------------------------------------
    ; Prolog
    ; -----------------------------------------------------
    push    rbp
    mov     rbp, rsp
    sub     rsp, 64            ; rezerwujemy miejsce na zmienne lokalne

    ; -----------------------------------------------------
    ; Zapisujemy 4 g��wne argumenty do pami�ci lokalnej
    ; -----------------------------------------------------
    mov     [rsp +  0], rcx    ; [rsp +  0] = bitmapPhoto
    mov     [rsp +  8], rdx    ; [rsp +  8] = originalWidth
    mov     [rsp + 16], r8     ; [rsp + 16] = originalHeight
    mov     [rsp + 24], r9     ; [rsp + 24] = newWidth

    ; -----------------------------------------------------
    ; Wczytanie 5. argumentu (newHeight, int)
    ; UWAGA: W Windows x64 kolejne argumenty (po 4.) trafiaj� na stos.
    ;        Dlatego musimy pobra� *tylko* 32 bity z w�a�ciwej pozycji.
    ;        Je�eli w C++ mamy 'int newHeight', to musimy zrobi� mov eax, [rbp+?].
    ; -----------------------------------------------------

    ; Najcz�ciej 5-ty argument (typu 32-bit int) jest w [rbp + 48],
    ; ale to zale�y od uk�adu stosu w czasie wywo�ania.
    ; W praktyce, przy "push rbp / mov rbp, rsp", 
    ;   [rbp +  0] = poprzedni RBP
    ;   [rbp +  8] = adres powrotny
    ;   [rbp + 16..] = home space/rejestry
    ;   itd.
    ;
    ; Je�li Tw�j kompilator/�rodowisko potwierdza, �e 5-ty argument
    ; jest pod [rbp + 48], wczytujemy go tak:
    ;

    mov     eax, DWORD PTR [rbp + 48] ; za�aduj 32 bity newHeight do EAX
    mov     [rsp + 32], rax           ; zapisz w [rsp+32] (jako int, z wyzerowan� g�r� RAX)

    ; ------------------------------
    ; Sprawdzenie warunk�w b��dnych (NULL, <= 0, itp.)
    ; ------------------------------

    ; 1. bitmapPhoto (64-bit wska�nik), sprawdzamy != NULL
    mov   rax, [rsp +  0]    ; wczytujemy wska�nik
    test  rax, rax           ; sprawdzamy, czy jest r�wny 0
    jz    error              ; je�li 0 -> b��d

    ; 2. originalWidth (32-bit int), sprawdzamy > 0
    mov   eax, [rsp +  8]    ; wczytujemy 32 bity do EAX
    test  eax, eax           ; sprawdzamy znak i zero
    jle   error              ; je�li <= 0 -> b��d

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
    and     rax, -4            ; wyr�wnanie w d� (do wielokrotno�ci 4)
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
    ; Wywo�anie VirtualAlloc (alokacja pami�ci)
    ; W MS x64 parametry s� w rejestrach: rcx, rdx, r8, r9
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

    ; Zapisujemy wska�nik do zaalokowanej pami�ci
    mov     [rsp + 56], rax    ; resizedBitmap

    ; -----------------------------------------------------
    ; Skalowanie metod� najbli�szego s�siada
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
    ; Wska�nik �r�d�owy: 
    ;   sourcePtr = bitmapPhoto + ((srcY * originalWidth) + srcX) * 3
    ; -------------------------------------------------
    mov     rax, [rsp +  0]    ; base = bitmapPhoto

    mov     r8, rdi            ; r8 = srcY
    imul    r8, [rsp +  8]     ; r8 *= originalWidth
    imul    r8, 3
    add     rax, r8            ; rax -> pocz�tek wiersza

    mov     r8, rsi            ; r8 = srcX
    imul    r8, 3
    add     rax, r8            ; rax -> (srcX) w tym wierszu

    ; Zachowujemy wska�nik �r�d�a w r10
    mov     r10, rax

    ; -------------------------------------------------
    ; Wska�nik docelowy:
    ;   destPtr = resizedBitmap + (y * newRowSize) + (x * 3)
    ; -------------------------------------------------
    mov     rbx, [rsp + 56]    ; rbx = resizedBitmap

    mov     r8, [rsp + 40]     ; r8 = newRowSize
    imul    r8, rcx            ; y * newRowSize
    add     rbx, r8            ; pocz�tek wiersza docelowego

    mov     r8, rdx            ; x
    imul    r8, 3
    add     rbx, r8            ; offset w wierszu

    ; -------------------------------------------------
    ; Kopiowanie 3 bajt�w: B, G, R
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
    ; Zwracamy wska�nik do nowego obrazu w RAX
    ; -----------------------------------------------------
    mov     rax, [rsp + 56]

    ; -----------------------------------------------------
    ; Epilog
    ; -----------------------------------------------------
    mov     rsp, rbp
    pop     rbp
    ret

; -----------------------------------------------------
; Obs�uga b��d�w:
; -----------------------------------------------------
error:
    xor     rax, rax   ; RAX = 0 (NULL)
    mov     rsp, rbp
    pop     rbp
    ret

ScaleImageAsm ENDP
END
