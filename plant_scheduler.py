from datetime import datetime, timedelta
from typing import Any, Dict
from firebase_client import FirebaseClient
from notification_service import NotificationService


class PlantScheduler:
    def __init__(self, firebase_client: FirebaseClient, notification_service: NotificationService):
        self.firebase_client = firebase_client
        self.notification_service = notification_service
        self.event_messages = {
            "watering": {
                "title": "Полив",
                "body_template": "Полейте {plant_name}"
            }
        }

    def check_and_send_notifications(self, event : str):
        users = self.firebase_client.get_object_from_db("Users")
        print(users)
        if users is None:
            return
        today = datetime.now()
        for user_id, user_data in users.items():
            device_token = user_data.get("fcm_token")
            if not device_token:
                continue

            for ind, plant_data in enumerate(user_data.get("plants", [])):
                if plant_data.get("is_notify") == True:
                    plant_name = plant_data.get("name")
                    if not plant_name:
                        continue

                    last_watering = plant_data.get(f"last_{event}")
                    if not last_watering:
                        self.update_last_notification(plant_data, today, user_id, ind, event)
                        continue

                    freq_str = plant_data.get(event)
                    if not freq_str:
                        print(f"У растения '{plant_name}' отсутствует интервал {event}.")
                        continue
                    interval = timedelta(hours=int(freq_str))

                    last_watering_datetime = datetime.fromisoformat(last_watering)

                    if today - last_watering_datetime >= interval:
                        message_body = self.event_messages[event][
                            "body_template"].format(plant_name=plant_name)
                        if self.notification_service.send_notification(
                            device_token,
                            self.event_messages[event]["title"],
                            message_body
                        ):
                            self.update_last_notification(plant_data, today, user_id, ind, event)

    def update_last_notification(self, plant_data: Dict[str, Any], today: datetime, user_id: str, ind: int, event : str):
        plant_data[f"last_{event}"] = today.isoformat()
        path = f"Users/{user_id}/plants/{ind}"
        success = self.firebase_client.update_object_in_db(path, plant_data)
        if success:
            print(f"Обновлено время последнего полива для растения '{plant_data.get('Name')}' пользователя '{user_id}'.")
        else:
            print(f"Не удалось обновить время последнего полива для растения '{plant_data.get('Name')}' пользователя '{user_id}'.")
