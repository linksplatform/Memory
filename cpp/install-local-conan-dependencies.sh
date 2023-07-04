#!/bin/bash

git clone https://github.com/linksplatform/conan-center-index
cd conan-center-index && cd recipes
conan create platform.collections/all --version 0.2.1
