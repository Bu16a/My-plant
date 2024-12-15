import schedule
import time
from plant_watering_scheduler import PlantWateringScheduler


class Scheduler:
    def __init__(self, plant_watering_scheduler: PlantWateringScheduler):
        self.plant_watering_scheduler = plant_watering_scheduler

    def start(self):
        schedule.every(5).minutes.do(self.plant_watering_scheduler.check_and_send_notifications, event="Watering")

        print("Планировщик запущен. Ожидание заданий...")

        while True:
            schedule.run_pending()
            time.sleep(1)
