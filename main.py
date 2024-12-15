from firebase_client import FirebaseClient
from notification_service import NotificationService
from plant_watering_scheduler import PlantWateringScheduler
from scheduler import Scheduler


def main():
    # Инициализация компонентов
    firebase_client = FirebaseClient(
        credential_path="uplant-36fdf-firebase-adminsdk-5pkef-9fd4996e6f.json",
        database_url="https://uplant-36fdf-default-rtdb.europe-west1.firebasedatabase.app/"
    )
    notification_service = NotificationService()
    plant_watering_scheduler = PlantWateringScheduler(firebase_client, notification_service)
    scheduler = Scheduler(plant_watering_scheduler)

    # Запуск планировщика
    scheduler.start()


if __name__ == "__main__":
    main()
