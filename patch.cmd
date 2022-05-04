@echo off

DEL /q bin\noise.exe

git reset --hard

git clean -fd

git pull origin main

cmd build.cmd