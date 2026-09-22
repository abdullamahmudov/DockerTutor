#!/usr/bin/env python3
"""
Скрипт для автоматической загрузки моделей из Hugging Face
Автоматически извлекает source и model из URL

Пример использования:
    python download_model.py --url https://huggingface.co/unsloth/GLM-4.7-Flash-GGUF/blob/main/GLM-4.7-Flash-UD-Q5_K_XL.gguf --path ./models
"""

import argparse
import subprocess
import sys
import os
import re
import threading
import time

def parse_hf_url(url):
    """
    Парсинг URL Hugging Face для извлечения source и model
    
    Args:
        url (str): URL модели в Hugging Face
        
    Returns:
        tuple: (source, model) или (None, None) если не удалось распарсить
    """
    # Примеры URL:
    # https://huggingface.co/unsloth/GLM-4.7-Flash-GGUF/blob/main/GLM-4.7-Flash-UD-Q5_K_XL.gguf
    # https://huggingface.co/organization/repo_name/blob/main/model_file.gguf
    
    pattern = r'https://huggingface\.co/([^/]+)/([^/]+)/blob/main/(.+)'
    match = re.match(pattern, url)
    
    if match:
        source = f"{match.group(1)}/{match.group(2)}"
        model = match.group(3)
        return source, model
    else:
        # Попробуем другой паттерн для разных форматов URL
        pattern2 = r'https://huggingface\.co/([^/]+)/([^/]+)/(.+)'
        match2 = re.match(pattern2, url)
        if match2:
            source = f"{match2.group(1)}/{match2.group(2)}"
            model = match2.group(3).split('/')[-1]  # Берем последнюю часть как имя файла
            return source, model
    
    return None, None

def download_model(source, model, path):
    """
    Загрузка модели из Hugging Face с отображением прогресса
    
    Args:
        source (str): Источник модели (например, unsloth/GLM-4.7-Flash-GGUF)
        model (str): Название файла модели
        path (str): Путь для сохранения модели
    
    Returns:
        bool: True если успешно, False в противном случае
    """
    try:
        # Создаем директорию если она не существует
        os.makedirs(path, exist_ok=True)
        
        # Формируем команду для загрузки
        command = [
            'hf', 'download',
            source,
            '--include', model,
            '--local-dir', path
        ]
        
        print(f"🚀 Начинаем загрузку модели...")
        print(f"📦 Источник: {source}")
        print(f"📄 Модель: {model}")  
        print(f"📁 Путь: {path}")
        print("-" * 50)
        print("⏳ Загрузка начата, пожалуйста подождите...")
        print()
        
        # Выполняем команду с отображением вывода в реальном времени
        process = subprocess.Popen(
            command,
            stdout=subprocess.PIPE,
            stderr=subprocess.STDOUT,
            universal_newlines=True,
            bufsize=1
        )
        
        # Читаем и отображаем вывод построчно с принудительной прокруткой буфера
        while True:
            output = process.stdout.readline()
            if output == '' and process.poll() is not None:
                break
            if output:
                # Принудительно flush stdout для PowerShell
                print(output.strip(), flush=True)
        
        return_code = process.poll()
        
        if return_code == 0:
            print()
            print("-" * 50)
            print("✅ Загрузка завершена успешно!")
            return True
        else:
            print()
            print("-" * 50)
            print("❌ Ошибка при загрузке модели")
            return False
        
    except subprocess.CalledProcessError as e:
        print(f"❌ Ошибка при загрузке модели: {e}")
        print(f"STDERR: {e.stderr}")
        return False
    except Exception as e:
        print(f"❌ Неожиданная ошибка: {e}")
        return False

def main():
    parser = argparse.ArgumentParser(description='Загрузка моделей из Hugging Face')
    parser.add_argument('--url', required=True, help='URL модели в Hugging Face')
    parser.add_argument('--path', required=True, help='Путь для сохранения модели')
    
    args = parser.parse_args()
    
    # Парсим URL для получения source и model
    source, model = parse_hf_url(args.url)
    
    if not source or not model:
        print(f"❌ Не удалось распарсить URL: {args.url}")
        print("Поддерживаемые форматы URL:")
        print("  https://huggingface.co/organization/repo_name/blob/main/model_file.gguf")
        sys.exit(1)
    
    success = download_model(source, model, args.path)
    
    if not success:
        sys.exit(1)

if __name__ == "__main__":
    main()