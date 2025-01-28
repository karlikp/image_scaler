; Definicja segmentu danych
.DATA
resizedBitmap QWORD ? ; Bufor dla nowego obrazu

; Deklaracje procedur
EXTERN VirtualAlloc:PROC

; Definicja segmentu kodu
.CODE

PUBLIC ScaleImageAsm

; Funkcja: ScaleImageAsm
; Argumenty:
;   - bitmapPhoto: wskaŸnik do oryginalnego obrazu
;   - originalWidth: szerokoœæ oryginalnego obrazu
;   - originalHeight: wysokoœæ oryginalnego obrazu
;   - newWidth: nowa szerokoœæ
;   - newHeight: nowa wysokoœæ
; Zwraca:
;   - wskaŸnik do nowego, skalowanego obrazu lub NULL w przypadku b³êdu

ScaleImageAsm PROC
    ; Prolog funkcji
    push rbp
    mov rbp, rsp
    sub rsp, 64                ; Alokacja miejsca na zmienne lokalne

    ; Pobranie argumentów
    mov rax, [rbp + 16]        ; bitmapPhoto
    mov [rsp + 0], rax
    mov rax, [rbp + 24]        ; originalWidth
    mov [rsp + 8], rax
    mov rax, [rbp + 32]        ; originalHeight
    mov [rsp + 16], rax
    mov rax, [rbp + 40]        ; newWidth
    mov [rsp + 24], rax
    mov rax, [rbp + 48]        ; newHeight
    mov [rsp + 32], rax

    ; Sprawdzenie warunków b³êdnych
    cmp qword ptr [rsp + 0], 0 ; if (!bitmapPhoto)
    je error
    cmp qword ptr [rsp + 8], 0 ; if (originalWidth <= 0)
    jle error
    cmp qword ptr [rsp + 16], 0 ; if (originalHeight <= 0)
    jle error
    cmp qword ptr [rsp + 24], 0 ; if (newWidth <= 0)
    jle error
    cmp qword ptr [rsp + 32], 0 ; if (newHeight <= 0)
    jle error

    ; Obliczenie newRowSize = (newWidth * 3 + 3) & ~3
    mov rax, [rsp + 24]        ; newWidth
    imul rax, 3                ; newWidth * 3
    add rax, 3
    and rax, 0FFFFFFFFFFFFFFFCh ; Wyrównanie do 4 bajtów
    mov [rsp + 40], rax        ; newRowSize

    ; Obliczenie newPixelArraySize = newRowSize * newHeight
    mov rbx, rax               ; newRowSize
    mov rax, [rsp + 32]        ; newHeight
    imul rax, rbx              ; newRowSize * newHeight
    mov [rsp + 48], rax        ; newPixelArraySize

    ; Alokacja pamiêci dla resizedBitmap za pomoc¹ VirtualAlloc
    push 64                  ; PAGE_READWRITE
    push 4096                ; MEM_COMMIT
    mov rax, [rsp + 48]      ; Rozmiar nowego bufora
    push rax                 ; newPixelArraySize
    push 0                   ; Brak konkretnego adresu bazowego (system wybierze)
    call VirtualAlloc
    test rax, rax            ; Sprawdzenie czy alokacja siê powiod³a
    jz error
    mov [rsp + 56], rax      ; resizedBitmap

    ; Skalowanie obrazu (interpolacja najbli¿szego s¹siada)
    xor rcx, rcx             ; y = 0
outer_loop:
    cmp rcx, [rsp + 32]      ; y < newHeight
    jge end_outer_loop

    xor rdx, rdx             ; x = 0
inner_loop:
    cmp rdx, [rsp + 24]      ; x < newWidth
    jge end_inner_loop

    ; Obliczenie srcX = (x * originalWidth) / newWidth
    mov rax, rdx             ; x
    imul rax, [rsp + 8]      ; x * originalWidth
    cqo
    idiv qword ptr [rsp + 24] ; (x * originalWidth) / newWidth
    mov rsi, rax             ; srcX

    ; Obliczenie srcY = (y * originalHeight) / newHeight
    mov rax, rcx             ; y
    imul rax, [rsp + 16]     ; y * originalHeight
    cqo
    idiv qword ptr [rsp + 32] ; (y * originalHeight) / newHeight
    mov rdi, rax             ; srcY

    ; Obliczenie wskaŸnika do piksela Ÿród³owego
    mov rax, [rsp + 0]       ; bitmapPhoto
    mov rbx, [rsp + 8]       ; originalWidth
    imul rdi, rbx            ; srcY * originalWidth
    imul rdi, 3              ; (srcY * originalWidth) * 3
    add rax, rdi
    lea rax, [rax + rsi * 2] ; bitmapPhoto + srcY * originalWidth * 2
    add rax, rsi             ; bitmapPhoto + srcY * originalWidth * 3

    ; Obliczenie wskaŸnika do piksela docelowego
    mov rbx, [rsp + 56]      ; resizedBitmap
    mov rdi, [rsp + 40]      ; newRowSize
    imul rcx, rdi            ; y * newRowSize
    lea rbx, [rbx + rcx]      ; resizedBitmap + y * newRowSize
    lea rax, [rdx + rdx * 2]  ; rdx * 3
    add rbx, rax              ; resizedBitmap + y * newRowSize + x * 3


    ; Kopiowanie piksela (BGR)
    movzx rsi, byte ptr [rax]      ; Blue
    mov [rbx], sil
    movzx rsi, byte ptr [rax + 1]  ; Green
    mov [rbx + 1], sil
    movzx rsi, byte ptr [rax + 2]  ; Red
    mov [rbx + 2], sil

    inc rdx                  ; x++
    jmp inner_loop
end_inner_loop:
    inc rcx                  ; y++
    jmp outer_loop
end_outer_loop:

    ; Epilog funkcji
    mov rax, [rsp + 56]      ; Zwracamy wskaŸnik na dane obrazu
    mov rsp, rbp
    pop rbp
    ret

error:
    mov rax, 0               ; Zwracamy NULL w przypadku b³êdu
    mov rsp, rbp
    pop rbp
    ret

ScaleImageAsm ENDP
END
