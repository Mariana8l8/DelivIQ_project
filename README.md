Інструкція з запуску проєкту
1. Необхідне програмне забезпечення

Перед початком роботи на комп’ютері має бути встановлено:

Visual Studio 2022 (версія 17.x або новіша)

Робоче навантаження ASP.NET and Web Development

.NET SDK 8.0

Microsoft SQL Server (LocalDB або повна версія)

Git (для клонування репозиторія)

2. Клонування репозиторія

Відкрити Command Prompt або Git Bash

Перейти до папки, де буде збережено проєкт

Виконати команду:

git clone <URL_репозиторія>


Після завершення клонування перейти до папки проєкту

3. Відкриття проєкту у Visual Studio

Запустити Visual Studio

Обрати пункт Open a project or solution

Відкрити файл рішення з розширенням .sln

Дочекатися завершення завантаження всіх залежностей

4. Налаштування підключення до бази даних

Відкрити файл appsettings.json

Перевірити або змінити рядок підключення:

"ConnectionStrings": {
  "DefaultConnection": "Server=(localdb)\\MSSQLLocalDB;Database=DelivIQDb;Trusted_Connection=True;"
}


Переконатися, що SQL Server доступний на локальному комп’ютері

5. Застосування міграцій бази даних

У Visual Studio відкрити:

Tools → NuGet Package Manager → Package Manager Console


Обрати проєкт, що містить DbContext

Виконати команду:

Update-Database


Після виконання команди база даних буде створена автоматично

6. Запуск проєкту

У верхній панелі Visual Studio обрати профіль IIS Express або https

Натиснути кнопку Run або клавішу F5

Після запуску у браузері відкриється вебзастосунок
