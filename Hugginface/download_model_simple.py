#!/usr/bin/env python3
"""
Простой скрипт для загрузки моделей из Hugging Face с отображением прогресса
"""

import argparse
import subprocess
import sys
import os

def main():
    parser = argparse.ArgumentParser(description='Загрузка моделей из Hugging Face')
    parser.add_argument('--url', required=True, help='URL модели в Hugging Face')
    parser.add_argument('--path', required=True, help='Путь для сохранения модели')
    
    args = parser.parse_args()
    
    # Парсим URL
    import re
    pattern = r'https://huggingface\.co/([^/]+)/([^/]+)/blob/main/(.+)'
    match = re.match(pattern, args.url)
    
    if not match:
        print(f"❌ Не удалось распарсить URL: {args.url}")
        print("Поддерживаемые форматы URL:")
        print("  https://huggingface.co/organization/repo_name/blob/main/model_file.gguf")
        sys.exit(1)
    
    source = f"{match.group(1)}/{match.group(2)}"
    model = match.group(3)
    
    # Создаем директорию если она не существует
    os.makedirs(args.path, exist_ok=True)
    
    print(f"🚀 Начинаем загрузку модели...")
    print(f"📦 Источник: {source}")
    print(f"📄 Модель: {model}")  
    print(f"📁 Путь: {args.path}")
    print("-" * 50)
    print("⏳ Загрузка начата, пожалуйста подождите...")
    print()
    
    # Выполняем команду напрямую - это должно работать корректно в PowerShell
    try:
        command = [
            'hf', 'download',
            source,
            '--include', model,
            '--local-dir', args.path
        ]
        
        # Запускаем команду с отображением вывода
        result = subprocess.run(command, check=True, capture_output=False, text=True)
        
        print()
        print("-" * 50)
        print("✅ Загрузка завершена успешно!")
        
    except subprocess.CalledProcessError as e:
        print(f"❌ Ошибка при загрузке модели: {e}")
        sys.exit(1)
    except Exception as e:
        print(f"❌ Неожиданная ошибка: {e}")
        sys.exit(1)

if __name__ == "__main__":
    main()