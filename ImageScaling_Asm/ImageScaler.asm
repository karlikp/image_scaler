; Definicja segmentu danych
.DATA
; Bufor dla nowego obrazu
resizedBitmap DWORD ?

; Definicja segmentu kodu
.CODE
; Deklaracje procedur
EXTERN malloc:PROC
PUBLIC ScaleImageAsm

; Funkcja: ScaleImageAsm
; Argumenty:
;   - bitmapPhoto: wska�nik do oryginalnego obrazu
;   - originalWidth: szeroko�� oryginalnego obrazu
;   - originalHeight: wysoko�� oryginalnego obrazu
;   - newWidth: nowa szeroko��
;   - newHeight: nowa wysoko��
; Zwraca:
;   - wska�nik do nowego, skalowanego obrazu lub NULL w przypadku b��du

ScaleImageAsm PROC
    ; Prolog funkcji
    push [ebp]
    mov [ebp], esp
    sub esp, 32                ; Alokacja miejsca na zmienne lokalne

    ; Pobranie argument�w
    mov eax, [ebp + 8]         ; bitmapPhoto
    mov [esp + 0], eax
    mov eax, [ebp + 12]        ; originalWidth
    mov [esp + 4], eax
    mov eax, [ebp + 16]        ; originalHeight
    mov [esp + 8], eax
    mov eax, [ebp + 20]        ; newWidth
    mov [esp + 12], eax
    mov eax, [ebp + 24]        ; newHeight
    mov [esp + 16], eax

    ; Sprawdzenie warunk�w b��dnych
    cmp dword ptr [esp + 0], 0     ; if (!bitmapPhoto)
    je error
    cmp dword ptr [esp + 4], 0     ; if (originalWidth <= 0)
    jle error
    cmp dword ptr [esp + 8], 0     ; if (originalHeight <= 0)
    jle error
    cmp dword ptr [esp + 12], 0    ; if (newWidth <= 0)
    jle error
    cmp dword ptr [esp + 16], 0    ; if (newHeight <= 0)
    jle error

    ; Obliczenie newRowSize = (newWidth * 3 + 3) & ~3
    mov eax, [esp + 12]        ; newWidth
    imul eax, 3                ; newWidth * 3
    add eax, 3
    and eax, 0FFFFFFFCh        ; Wyr�wnanie do 4 bajt�w
    mov [esp + 20], eax        ; newRowSize

    ; Obliczenie newPixelArraySize = newRowSize * newHeight
    mov ebx, eax               ; newRowSize
    mov eax, [esp + 16]        ; newHeight
    imul eax, ebx              ; newRowSize * newHeight
    mov [esp + 24], eax        ; newPixelArraySize

    ; Alokacja pami�ci dla resizedBitmap
    push [eax]                   ; newPixelArraySize
    call malloc
    add esp, 4
    test [eax], eax
    jz error
    mov [esp + 28], eax        ; resizedBitmap

    ; Skalowanie obrazu (interpolacja najbli�szego s�siada)
    xor ecx, ecx               ; y = 0
outer_loop:
    cmp ecx, [esp + 16]        ; y < newHeight
    jge end_outer_loop

    xor edx, edx               ; x = 0
inner_loop:
    cmp edx, [esp + 12]        ; x < newWidth
    jge end_inner_loop

    ; Obliczenie srcX = (x * originalWidth) / newWidth
    mov eax, edx               ; x
    imul eax, [esp + 4]        ; x * originalWidth
    cdq
    idiv dword ptr [esp + 12]  ; (x * originalWidth) / newWidth
    mov esi, eax               ; srcX

    ; Obliczenie srcY = (y * originalHeight) / newHeight
    mov eax, ecx               ; y
    imul eax, [esp + 8]        ; y * originalHeight
    cdq
    idiv dword ptr [esp + 16]  ; (y * originalHeight) / newHeight
    mov edi, eax               ; srcY

    ; Obliczenie wska�nika do piksela �r�d�owego
    mov eax, [esp + 0]         ; bitmapPhoto
    mov ebx, [esp + 4]         ; originalWidth
    imul edi, ebx              ; srcY * originalWidth
    imul edi, 3                ; (srcY * originalWidth) * 3
    add eax, edi
    add eax, esi
    imul eax, 3                ; srcPixel = bitmapPhoto + (srcY * originalWidth * 3) + (srcX * 3)

    ; Obliczenie wska�nika do piksela docelowego
    mov ebx, [esp + 28]        ; resizedBitmap
    mov edi, [esp + 20]        ; newRowSize
    imul ecx, edi              ; y * newRowSize
    add ebx, ecx
    add ebx, edx
    imul ebx, 3                ; destPixel = resizedBitmap + (y * newRowSize) + (x * 3)

    ; Kopiowanie piksela (BGR)
    movzx esi, byte ptr [eax]      ; Blue
    mov [ebx], sil
    movzx esi, byte ptr [eax + 1]  ; Green
    mov [ebx + 1], sil
    movzx esi, byte ptr [eax + 2]  ; Red
    mov [ebx + 2], sil

    inc edx                    ; x++
    jmp inner_loop
end_inner_loop:
    inc ecx                    ; y++
    jmp outer_loop
end_outer_loop:

    ; Epilog funkcji
    mov eax, [esp + 28]        ; Zwracamy wska�nik na dane obrazu
    mov esp, [ebp]
    pop [ebp]
    ret

error:
    mov eax, 0                 ; Zwracamy NULL w przypadku b��du
    mov esp, [ebp]
    pop [ebp]

ScaleImageAsm ENDP

END
 
