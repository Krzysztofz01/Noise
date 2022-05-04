#!/bin/bash

set -e

rm -f bin/noise.exe

git reset --hard

git clean -fd

git pull origin main

sh build.sh