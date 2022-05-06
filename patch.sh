#!/bin/bash

set -e

mv ./bin ./bin-pre

mkdir ./patch

git clone https://github.com/Krzysztofz01/Noise.git ./patch

sudo chmod +x ./patch/build.sh

sudo sh ./patch/build.sh

rm -f LICENSE

rm -f NOTICES

rm -f README.md

rm -f patch.sh

mv ./patch/* ./

mv ./bin-pre/peer.noise ./bin/peer.noise

rm ./bin-pre