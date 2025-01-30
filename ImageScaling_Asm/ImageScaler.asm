; ------------------------------------------------------------------------------
; You can disable case mapping if you use MASM:
option casemap:none

.code

PUBLIC ScaleImageAsm

; ------------------------------------------------------------------------------
; ScaleImageAsm PROC
; Parameters in MS x64 ABI:
;   RCX -> input   (const uint32_t*)
;   RDX -> output  (uint32_t*)
;   R8  -> iWidth  (size_t)
;   R9  -> iHeight (size_t)
;   [rsp+32] -> oWidth  (size_t)
;   [rsp+40] -> oHeight (size_t)
; ------------------------------------------------------------------------------
ScaleImageAsm PROC

        ; Reserve 32 bytes of shadow space (Microsoft x64 ABI requirement)
        sub     rsp, 32

        ; Retrieve the 5th and 6th argument from the stack
        mov     r10d, [rbp + 48]      ; r10 = oWidth
        mov     r11d, [rbp + 56]      ; r11 = oHeight

        ; Now we want to map the old registers:
        ;   (the old code assumed: rdi=input, rsi=output, rdx=iWidth, rcx=iHeight, r8=oWidth, r9=oHeight)
        ;
        ; In MS x64:
        ;   RCX = input, RDX = output, R8 = iWidth, R9 = iHeight, r10 = oWidth, r11 = oHeight
        ;
        ; So we "rearrange" them this way:
        mov     rdi, rcx    ; rdi = input
        mov     rsi, rdx    ; rsi = output
        mov     rdx, r8     ; iWidth
        mov     rcx, r9     ; iHeight
        mov     r8,  r10    ; oWidth
        mov     r9,  r11    ; oHeight

        ; ------------------------------------------------------------------------------
        ; From here on, the code can be almost identical to the System V version
        ; ------------------------------------------------------------------------------
        ; xRatio = float(iWidth) / oWidth
        cvtsi2ss xmm0, rdx       ; xmm0 = (float)iWidth
        cvtsi2ss xmm1, r8        ; xmm1 = (float)oWidth
        divss   xmm0, xmm1       ; xmm0 = xRatio

        ; yRatio = float(iHeight) / oHeight
        cvtsi2ss xmm2, rcx       ; xmm2 = (float)iHeight
        cvtsi2ss xmm3, r9        ; xmm3 = (float)oHeight
        divss   xmm2, xmm3       ; xmm2 = yRatio

        ; r10 = y (outer loop)
        xor     r10, r10         ; y = 0

    outer_loop_y:
        cmp     r10, r9          ; if (y >= oHeight) -> end
        jae     end_function

        xor     r11, r11         ; x = 0 (pêtla wewnêtrzna)

    inner_loop_x:
        cmp     r11, r8          ; if (x >= oWidth) -> next y
        jae     end_inner_loop

        ;1.1 nearestX = (int)(x * xRatio)
        cvtsi2ss xmm4, r11
        mulss   xmm4, xmm0
        cvttss2si r12, xmm4

        ;2.1 nearestY = (int)(y * yRatio)
        cvtsi2ss xmm5, r10
        mulss   xmm5, xmm2
        cvttss2si r13, xmm5

        ;1.2 nearestX = min(nearestX, iWidth - 1)
        cmp     r12, rdx
        jb      no_clamp_x
        lea     r12, [rdx - 1]
    no_clamp_x:

        ;2.2 nearestY = min(nearestY, iHeight - 1)
        cmp     r13, rcx
        jb      no_clamp_y
        lea     r13, [rcx - 1]
    no_clamp_y:

        ;3 Load pixel from input[nearestY * iWidth + nearestX]
        mov     rax, r13
        imul    rax, rdx
        add     rax, r12
        mov     ebx, DWORD PTR [rdi + rax*4]  ; ebx = input[...]
        
        ;4 Store pixel to output[y * oWidth + x]
        mov     rax, r10
        imul    rax, r8
        add     rax, r11
        mov     DWORD PTR [rsi + rax*4], ebx  ; output[...]

        ; x++
        inc     r11
        jmp     inner_loop_x

    end_inner_loop:
        ; y++
        inc     r10
        jmp     outer_loop_y

    end_function:
        xor     eax, eax         ; return 0
        add     rsp, 32          ; restore stack pointer
        ret

ScaleImageAsm ENDP

END
