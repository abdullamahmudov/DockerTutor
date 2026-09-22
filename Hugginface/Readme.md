# Загрузка моделей из Hugging Face

Скрипты для автоматической загрузки моделей из Hugging Face.

## Файлы

- `download_model.py` - Python скрипт для загрузки моделей=

## Использование Python скрипта

Скрипт автоматически извлекает source и model из URL:

```bash
python download_model_simple.py --url https://huggingface.co/unsloth/GLM-4.7-Flash-GGUF/blob/main/GLM-4.7-Flash-UD-Q5_K_XL.gguf --path ./models
```

```bash
py -3 .\Hugginface\download_model_simple.py --url https://huggingface.co/unsloth/GLM-4.7-Flash-GGUF/blob/main/GLM-4.7-Flash-UD-Q5_K_XL.gguf --path .\llamaCPP\models\
```

Также можно использовать классический способ с указанием source и model отдельно:

```bash
python download_model.py --source unsloth/GLM-4.7-Flash-GGUF --model GLM-4.7-Flash-UD-Q5_K_XL.gguf --path ./models
```

## Особенности

### Автоматический парсинг URL (рекомендуемый способ)
Скрипт автоматически анализирует URL и извлекает:
- **Источник**: unsloth/GLM-4.7-Flash-GGUF  
- **Модель**: GLM-4.7-Flash-UD-Q5_K_XL.gguf

Поддерживаемые форматы URL:
```
https://huggingface.co/organization/repo_name/blob/main/model_file.gguf
```

## Примеры использования

Для загрузки модели из указанного URL:
- URL: https://huggingface.co/unsloth/GLM-4.7-Flash-GGUF/blob/main/GLM-4.7-Flash-UD-Q5_K_XL.gguf
- Путь: ./models (или любой другой путь)

Оба скрипта поддерживают:
- Создание директорий при необходимости
- Обработку ошибок
- Вывод информации о процессе загрузки

## Примеры команд Hugging Face CLI

```bash
# Базовая команда загрузки
hf download unsloth/GLM-4.7-Flash-GGUF --include GLM-4.7-Flash-UD-Q5_K_XL.gguf --local-dir ./models

# Загрузка с указанием конкретной папки
hf download {repo_id} --local-dir {/path/to/folder}

# Скачать только определенный файл 
huggingface-cli download repo_id --include "model.safetensors"

# Исключить тяжелые файлы
huggingface-cli download repo_id --exclude "*.safetensors"
```