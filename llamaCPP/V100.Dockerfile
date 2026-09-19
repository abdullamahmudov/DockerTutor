# Драйвер 475.06 поддерживает CUDA 11.x, поэтому берём образ CUDA 11.8.0: с драйвером
# >= 450 он работает за счёт совместимости минорных версий.
# Образа 11.4.3 под Ubuntu 22.04 не существует: 11.4.3 есть только под ubuntu20.04
# (CUDA_VERSION=11.4.3 UBUNTU_VERSION=20.04). CUDA 12.x на драйвере 475 не запустится.
ARG CUDA_VERSION=11.4.3
ARG UBUNTU_VERSION=20.04
 
# ---------- сборка ----------
FROM nvidia/cuda:${CUDA_VERSION}-devel-ubuntu${UBUNTU_VERSION} AS build
ENV DEBIAN_FRONTEND=noninteractive

ARG LLAMA_CPP_REF=master
ARG CUDA_ARCH=70
 
RUN apt-get update && apt-get install -y --no-install-recommends \
        git build-essential ca-certificates python3-pip \
    && pip3 install --no-cache-dir "cmake>=3.24,<4" \
    && rm -rf /var/lib/apt/lists/*
 
WORKDIR /src
RUN git clone https://github.com/ggml-org/llama.cpp.git . \
    && git checkout ${LLAMA_CPP_REF}
 
# libcuda.so.1 при сборке в контейнере нет (её подставит NVIDIA Container Toolkit
# при запуске), поэтому линкеру разрешаем неразрешённые символы в .so
RUN cmake -B build \
        -DCMAKE_BUILD_TYPE=Release \
        -DGGML_CUDA=ON \
        -DCMAKE_CUDA_ARCHITECTURES=${CUDA_ARCH} \
        -DLLAMA_CURL=OFF \
        -DCMAKE_EXE_LINKER_FLAGS=-Wl,--allow-shlib-undefined \
    && cmake --build build --config Release -j"$(nproc)" \
        --target llama-server llama-cli llama-bench
 
# ---------- запуск ----------
FROM nvidia/cuda:${CUDA_VERSION}-runtime-ubuntu${UBUNTU_VERSION} AS runtime
ENV DEBIAN_FRONTEND=noninteractive
 
RUN apt-get update && apt-get install -y --no-install-recommends libgomp1 \
    && rm -rf /var/lib/apt/lists/*
 
COPY --from=build /src/build/bin/ /app/
ENV LD_LIBRARY_PATH=/app
 
EXPOSE 8080
ENTRYPOINT ["/app/llama-server"]