import g4f
from g4f.client import Client
import asyncio

async def get_gpt_response(user_query: str, model=g4f.models.default):
    client = Client()
    try_count = 3  # Количество попыток
    response = None

    for attempt in range(try_count):
        try:
            # Отправляем текстовый запрос, используя выбранную модель
            response = await client.chat.completions.async_create(
                model=model,
                messages=[
                    {
                        "role": "user",
                        "content": user_query
                    }
                ]
            )

            if response:
                break  # Успешный ответ, выходим из цикла

        except Exception as e:
            print(f"Попытка {attempt + 1} не удалась: {e}")

    # Проверяем, что ответ получен и содержит нужную информацию
    if response and response.choices:
        return response.choices[0].message.content
    else:
        return "Не удалось получить ответ после нескольких попыток."
