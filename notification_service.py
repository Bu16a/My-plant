import json
from firebase_admin import messaging

class NotificationService:
    def __init__(self):
        pass

    def send_notification(self, device_token: str, title: str, body: str) -> bool:
        """
        Отправляет уведомление на устройство через FCM.
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
            return False
