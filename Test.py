import g4f
import requests
import asyncio
from g4f.client import Client

async def main():
    client = Client()

    image_url = "https://www.alexorg.biz/component/ajax/?p=image&src=WyJpbWFnZXNcL2FydGlzdHNcLzE2M29ubXluZWNrXC8xNjNvbm15bmVjay5qcGciLFtbImRvUmVzaXplIixbMTYwMCw5MDAsMTYwMCw5MDBdXSxbImRvQ3JvcCIsWzE2MDAsOTAwLDAsMF1dLFsidHlwZSIsWyJ3ZWJwIiwiODUiXV1dXQ%3D%3D&hash=27ef7c83f8a3a687de0af511a7bcc16b"

    try_count = 3  # Количество попыток
    response = None

    for attempt in range(try_count):
        try:
            image = requests.get(image_url, stream=True)

            if image.status_code == 200:
                with open('image.jpg', 'wb') as f:
                    f.write(image.raw.read())

                with open('image.jpg', 'rb') as f:
                    response = await client.chat.completions.async_create(
                        model=g4f.models.default,
                        messages=[
                            {
                                "role": "user",
                                "content": "Дай мне характеристику бархатца, как домашнего растения"
                            }
                        ],
                        image=f
                    )

                if response:
                    break  # Успешный ответ, выходим из цикла
            else:
                print(f"Ошибка загрузки изображения, статус: {image.status_code}")

        except Exception as e:
            print(f"Попытка {attempt + 1} не удалась: {e}")

    if response:
        print(response.choices[0].message.content)
    else:
        print("Не удалось получить ответ после нескольких попыток.")

asyncio.run(main())
