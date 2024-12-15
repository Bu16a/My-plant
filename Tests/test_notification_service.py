import unittest
from unittest.mock import patch, MagicMock
from notification_service import NotificationService


class TestNotificationService(unittest.TestCase):
    def setUp(self):
        self.notification_service = NotificationService()

    @patch('firebase_admin.messaging.send')
    def test_send_notification_success(self, mock_send):
        mock_send.return_value = "message_id_123"

        result = self.notification_service.send_notification(
            device_token="test_token",
            title="Test Title",
            body="Test Body"
        )
        self.assertTrue(result)
        mock_send.assert_called_once()

    @patch('firebase_admin.messaging.send')
    def test_send_notification_failure(self, mock_send):
        mock_send.side_effect = Exception("FCM send failed")

        result = self.notification_service.send_notification(
            device_token="test_token",
            title="Test Title",
            body="Test Body"
        )
        self.assertFalse(result)
        mock_send.assert_called_once()


if __name__ == '__main__':
    unittest.main()