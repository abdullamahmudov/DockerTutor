https://huggingface.co/docs/huggingface_hub/guides/cli

```
powershell -ExecutionPolicy ByPass -c "irm https://hf.co/cli/install.ps1 | iex"
```

```
powershell -ExecutionPolicy ByPass -c "& ([scriptblock]::Create((irm https://hf.co/cli/install.ps1))) -ExcludeSkill"
```

```
hf download HuggingFaceH4/zephyr-7b-beta
```

Указать конкретную папку для сохранения
```
hf download {repo_id} --local-dir {/path/to/folder}
```

Скачать только определенный файл (например, веса в формате .safetensors или .gguf)
```
huggingface-cli download repo_id --include "model.safetensors"
```

Исключить тяжелые файлы, которые вам не нужны (например, оригинальные веса PyTorch, если качаете GGUF)
```
huggingface-cli download repo_id --exclude "*.safetensors"
```

```
hf download hf://openai-community/gpt2/config.json
```

```
hf download unsloth/gemma-4-31B-it-GGUF --include gemma-4-31B-it-UD-Q6_K_XL.gguf --local-dir D:\Work\Custom\DockerTutor\DockerTutor\llamaCPP\models
```

