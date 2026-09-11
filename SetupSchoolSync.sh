#!/bin/sh
set -eu
cd "$(dirname "$0")"
git submodule update --init --recursive
git config --local submodule.recurse true
printf '%s\n' 'SchoolSync ready. Future git pull updates initialized submodules.'
