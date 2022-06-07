#!/bin/bash

git clone https://github.com/linksplatform/conan-center-index
cd conan-center-index && git checkout only-development && cd recipes
conan create platform.collections/all platform.collections/0.1.0@ -pr=linksplatform
