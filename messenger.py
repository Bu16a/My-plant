import json

import firebase_admin
from firebase_admin import credentials, db, messaging
from datetime import datetime, timedelta
import schedule
import time

cred = credentials.Certificate("uplant-36fdf-firebase-adminsdk-5pkef-9fd4996e6f.json")

firebase_admin.initialize_app(
    cred,
    {
        "databaseURL": "https://uplant-36fdf-default-rtdb.europe-west1.firebasedatabase.app/"
    },
)


def get_object_from_db(path):
    try:
        ref = db.reference(path)
        data = ref.get()
        if data is None:
            print(f"Объект по пути '{path}' не найден.")
        return data
    except Exception as e:
        print(f"Ошибка при получении объекта: {e}")
        return None


def send_notification(device_token, title, body):
    """
    Отправляет уведомление на устройство через FCM.

    :param device_token: FCM токен устройства.
    :param title: Заголовок уведомления.
    :param body: Текст уведомления.
    """
    buttons = [
        {"title": "Полил!", "action": "watered"},
        {"title": "Не могу(", "action": "unwatered"},
    ]
    data_payload = {
        "buttons": json.dumps(buttons)
    }
    try:
        message = messaging.Message(
            notification=messaging.Notification(
                title=title,
                body=body
            ),
            token=device_token,
            data=data_payload
        )
        response = messaging.send(message)
        print(f"Уведомление успешно отправлено {body}! ID сообщения: {response}")
        return True
    except Exception as e:
        print(f"Ошибка отправки уведомления: {e}")


def check_and_send_notifications():
    users = get_object_from_db("Users")
    print(users)
    if users is None:
        return
    today = datetime.now()  # Текущее время
    for user_id, user_data in users.items():
        if "fcm_token" not in user_data.keys():
            continue

        device_token = user_data["fcm_token"]
        ind = 0
        for plant_data in user_data["plants"]:
            plant_name = plant_data["Name"]
            if "Last_watering" not in plant_data.keys():
                update_last_notification(plant_data, today, user_id, ind)
            last_watering = plant_data["Last_watering"]  # Последнее уведомление
            freq = plant_data["Watering"]  # Интервал в строковом формате HH:MM:SS

            # Преобразуем интервал в timedelta
            interval_timedelta = timedelta(hours=int(freq))

            # Преобразуем время последнего уведомления
            last_watering_datetime = datetime.fromisoformat(last_watering)

            # Проверяем, прошло ли достаточно времени
            if today - last_watering_datetime >= interval_timedelta:
                if send_notification(device_token, "Полив", f"Полейте {plant_name}"):
                    # Обновляем время последнего уведомления в Firebase
                    update_last_notification(plant_data, today, user_id, ind)
            ind += 1
    print(users)


def update_last_notification(plant_data, today, user_id, ind):
    plant_data["Last_watering"] = today.isoformat()
    plant_ref = db.reference(f"Users/{user_id}/plants/{ind}")
    plant_ref.update(plant_data)


if __name__ == "__main__":
    print(get_object_from_db("Users"))
    # schedule.every(5).minutes.do(check_and_send_notifications)
    #
    # print("Планировщик запущен. Ожидание заданий...")
    #
    # while True:
    #     schedule.run_pending()
    #     time.sleep(150)
