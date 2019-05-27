#NoEnv  ; Recommended for performance and compatibility with future AutoHotkey releases.
; #Warn  ; Enable warnings to assist with detecting common errors.
SendMode Input  ; Recommended for new scripts due to its superior speed and reliability.
SetWorkingDir %A_ScriptDir%  ; Ensures a consistent starting directory.

STDIN := FileOpen("*", "r")


F10::
clipboard := STDIN.ReadLine()
Send ^a
Sleep 300
Send ^v
Sleep 300
SendInput {enter}
Sleep 100