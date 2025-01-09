from enum import StrEnum


class Prompts(StrEnum):
    image_flower_prompt = ("Определи растения на изображении и выведи их названия через запятую. Используй самые точные " +
                           "и детализированные ботанические наименования. Например, вместо «Кактус» укажи «Клейстокактус штраусса», " +
                           "а вместо «роза» — «роза Глория Дей». Если ты видишь несколько разновидностей одного растения, укажи каждую. " +
                           "Если растений нет на изображении, выведи только слово «Растений_нет». Ответ должен содержать только названия " +
                           "через запятую. Пример: клематис Жакмана, эхеверия агавоидная, тюльпан Дарвина. Все названия выводи с большой буквы.")

    flower_instruction = (
            'Print the frequency of watering in hours for the specified plant, i.e. Answer by indicating only one number,' +
            ' number of hours. Example answer: 4. That is. your answer must be only a number without letters, only one' +
            ' number. Execute the indicated commands for: ')
