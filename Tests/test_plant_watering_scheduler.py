import unittest
from unittest.mock import MagicMock, patch
from datetime import datetime, timedelta
from plant_scheduler import PlantScheduler


class TestPlantWateringScheduler(unittest.TestCase):
    def setUp(self):
        self.firebase_client = MagicMock()
        self.notification_service = MagicMock()
        self.scheduler = PlantScheduler(self.firebase_client, self.notification_service)

    def test_check_and_send_notifications_no_users(self):
        self.firebase_client.get_object_from_db.return_value = None
        self.scheduler.check_and_send_notifications("watering")
        self.firebase_client.get_object_from_db.assert_called_once_with("Users")
        self.notification_service.send_notification.assert_not_called()

    def test_check_and_send_notifications_no_fcm_token(self):
        users = {
            "user1": {
                "plants": []
            }
        }
        self.firebase_client.get_object_from_db.return_value = users

        self.scheduler.check_and_send_notifications("watering")
        self.notification_service.send_notification.assert_not_called()

    def test_check_and_send_notifications_send_notification(self):
        users = {
            "user1": {
                "fcm_token": "test_token",
                "plants": [
                    {
                        "is_notify": True,
                        "name": "Aloe",
                        "last_watering": (datetime.now() - timedelta(hours=5)).isoformat(),
                        "watering": "4"
                    }
                ]
            }
        }
        self.firebase_client.get_object_from_db.return_value = users
        self.notification_service.send_notification.return_value = True

        self.scheduler.check_and_send_notifications("watering")

        self.notification_service.send_notification.assert_called_once_with(
            "test_token",
            "Полив",
            "Полейте Aloe"
        )
        self.firebase_client.update_object_in_db.assert_called_once()
        updated_data = users["user1"]["plants"][0]
        self.assertIn("last_watering", updated_data)

    def test_check_and_send_notifications_notification_failure(self):
        users = {
            "user1": {
                "fcm_token": "test_token",
                "plants": [
                    {
                        "is_notify": True,
                        "name": "Aloe",
                        "last_watering": (datetime.now() - timedelta(hours=5)).isoformat(),
                        "watering": "4"
                    }
                ]
            }
        }
        self.firebase_client.get_object_from_db.return_value = users
        self.notification_service.send_notification.return_value = False

        self.scheduler.check_and_send_notifications("watering")

        self.notification_service.send_notification.assert_called_once_with(
            "test_token",
            "Полив",
            "Полейте Aloe"
        )
        self.firebase_client.update_object_in_db.assert_not_called()

if __name__ == '__main__':
    unittest.main()