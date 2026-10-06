# BadBoyClicker

**[English](#english) | [Русский](#русский)**

---

## English

BadBoyClicker is an idle / clicker game made with Unity 6 for the **Yandex Games** web platform (WebGL).
The player clicks the cat, earns coins, buys upgrades, levels up through prestige and collects cards.

### Features

- **Core loop** — click income, passive auto-income and offline income
- **Shop** — upgrades with exponential pricing, paid items and rewarded-ad items
- **Player progression** — levels / prestige with permanent bonuses and feature unlocks
- **Daily login** rewards and **daily quests**
- **Card collections** and **chests** with randomized rewards
- **Customization** of the main character and background
- **Monetization** — rewarded ads, temporary ad bonuses and in-app purchases (Yandex Payments)
- **Cloud saves** through the Yandex Games SDK
- **Localization** — Russian and English
- Tutorial, audio settings, analytics

### Tech stack

| Area | Technology |
|---|---|
| Engine | Unity **6000.5.1f1**, Universal Render Pipeline |
| Dependency injection | Zenject (Extenject) |
| Animations | DOTween |
| Platform SDK | PluginYG2 (Yandex Games: saves, ads, payments, localization) |
| UI | uGUI + TextMesh Pro |
| Target platform | WebGL (Yandex Games) |

### Architecture

- **Dependency injection** — all services are bound in `Installers/GameInstaller.cs` and depend on interfaces (`IShopModel`, `IRewardedAdsService`, `IPurchaseSystem`, …).
- **MVP (Model–View–Presenter)** — every UI feature is split into a service/model, a passive view and a presenter. View folders end with `MVP` (`ShopMVP`, `ChestsMVP`, …).
- **Null Object views** — if a view is not assigned in the scene, a `*NullView` is bound instead, so a missing UI element never crashes the game.
- **Configuration through ScriptableObjects** — balance, rewards, quests, audio and localization live in `Assets/Data/Scriptables`, so game design changes do not need code changes.
- **Save system** — every feature exposes its own save data; `SaveYGController` collects it into one `GameSaveData` and writes it at most once per second.

### Project structure

```
Assets/Data/
├── Scenes/         # Main game scene
├── Scriptables/    # Game configs (balance, quests, shop, localization, ...)
├── Scripts/
│   ├── Core/           # Wallet, save system, ads, time, configs
│   ├── Installers/     # Zenject installers
│   ├── ShopMVP/        # Shop
│   ├── PlayerProgression/, DailyLogin/, DailyQuests/, QuestSystem/
│   ├── CardCollections/, Chests/, Customization/, AdBonusOffers/
│   ├── PurchaseSystem/, Analytics/, Localization/, Audio/, Tutorials/
│   └── ...MVP/         # Views and presenters for each feature
├── Sprites/, Audio/, Material/
BALANCE_NOTES.md    # Progression balance targets and simulation results
```

### What you need

- **Unity 6000.5.1f1** (install through Unity Hub; other Unity 6 versions may work but are not tested)
- **WebGL Build Support** module for Unity
- An IDE with C# support: Visual Studio, JetBrains Rider or VS Code
- *Optional, for publishing:* a Yandex Games developer account

Zenject, DOTween and PluginYG2 are already included in the repository — nothing else needs to be installed.

### Getting started

1. Clone the repository:
   ```bash
   git clone https://github.com/AndreyFH3/BadBoyClicker.git
   ```
2. Open the project folder in Unity Hub with Unity **6000.5.1f1**.
3. Open the scene `Assets/Data/Scenes/SampleScene.unity`.
4. Press **Play**. The PluginYG2 simulator emulates the Yandex SDK (saves, ads, purchases) inside the editor.

### Building for Yandex Games

1. **File → Build Profiles** → select **Web** (WebGL).
2. Check the PluginYG2 settings (modules, payments, WebGL template).
3. Build, archive the build folder into a `.zip` and upload it to the Yandex Games developer console.

### Game balance

Target timings and progression formulas are described in [`BALANCE_NOTES.md`](BALANCE_NOTES.md) (in Russian).

### AI-assisted development

This project was developed with AI tools (Claude Code) used for code review, refactoring, bug fixing and documentation, to deliver features faster. All design and architecture decisions were made and checked by the author.

### License

[MIT](LICENSE)

---

## Русский

BadBoyClicker — idle/кликер-игра на Unity 6 для веб-платформы **Яндекс Игры** (WebGL).
Игрок кликает по коту, зарабатывает монеты, покупает улучшения, повышает уровень через престиж и собирает карточки.

### Возможности

- **Основной цикл** — доход за клики, пассивный автодоход и офлайн-доход
- **Магазин** — улучшения с экспоненциальной ценой, платные товары и товары за рекламу
- **Прогрессия игрока** — уровни/престиж с постоянными бонусами и открытием новых функций
- **Ежедневные награды** за вход и **ежедневные задания**
- **Коллекции карточек** и **сундуки** со случайными наградами
- **Кастомизация** персонажа и фона
- **Монетизация** — реклама за вознаграждение, временные бонусы за рекламу и внутриигровые покупки (Yandex Payments)
- **Облачные сохранения** через Yandex Games SDK
- **Локализация** — русский и английский
- Обучение, настройки звука, аналитика

### Технологии

| Область | Технология |
|---|---|
| Движок | Unity **6000.5.1f1**, Universal Render Pipeline |
| Внедрение зависимостей | Zenject (Extenject) |
| Анимации | DOTween |
| SDK платформы | PluginYG2 (Яндекс Игры: сохранения, реклама, покупки, локализация) |
| UI | uGUI + TextMesh Pro |
| Целевая платформа | WebGL (Яндекс Игры) |

### Архитектура

- **Внедрение зависимостей** — все сервисы регистрируются в `Installers/GameInstaller.cs` и зависят от интерфейсов (`IShopModel`, `IRewardedAdsService`, `IPurchaseSystem`, …).
- **MVP (Model–View–Presenter)** — каждая UI-функция разделена на сервис/модель, пассивное представление и презентер. Папки с представлениями заканчиваются на `MVP` (`ShopMVP`, `ChestsMVP`, …).
- **Null Object представления** — если представление не назначено на сцене, вместо него подставляется `*NullView`, поэтому отсутствующий UI-элемент не ломает игру.
- **Конфигурация через ScriptableObject** — баланс, награды, задания, звук и локализация хранятся в `Assets/Data/Scriptables`, поэтому изменения геймдизайна не требуют правок кода.
- **Система сохранений** — каждая функция отдаёт свои данные сохранения; `SaveYGController` собирает их в один `GameSaveData` и записывает не чаще одного раза в секунду.

### Структура проекта

```
Assets/Data/
├── Scenes/         # Основная игровая сцена
├── Scriptables/    # Конфиги игры (баланс, задания, магазин, локализация, ...)
├── Scripts/
│   ├── Core/           # Кошелёк, сохранения, реклама, время, конфиги
│   ├── Installers/     # Zenject-инсталлеры
│   ├── ShopMVP/        # Магазин
│   ├── PlayerProgression/, DailyLogin/, DailyQuests/, QuestSystem/
│   ├── CardCollections/, Chests/, Customization/, AdBonusOffers/
│   ├── PurchaseSystem/, Analytics/, Localization/, Audio/, Tutorials/
│   └── ...MVP/         # Представления и презентеры для каждой функции
├── Sprites/, Audio/, Material/
BALANCE_NOTES.md    # Цели баланса прогрессии и результаты симуляции
```

### Что нужно

- **Unity 6000.5.1f1** (устанавливается через Unity Hub; другие версии Unity 6 могут работать, но не проверялись)
- Модуль **WebGL Build Support** для Unity
- IDE с поддержкой C#: Visual Studio, JetBrains Rider или VS Code
- *Необязательно, для публикации:* аккаунт разработчика Яндекс Игр

Zenject, DOTween и PluginYG2 уже включены в репозиторий — больше ничего устанавливать не нужно.

### Как запустить

1. Клонируйте репозиторий:
   ```bash
   git clone https://github.com/AndreyFH3/BadBoyClicker.git
   ```
2. Откройте папку проекта в Unity Hub с Unity **6000.5.1f1**.
3. Откройте сцену `Assets/Data/Scenes/SampleScene.unity`.
4. Нажмите **Play**. Симулятор PluginYG2 эмулирует Yandex SDK (сохранения, рекламу, покупки) прямо в редакторе.

### Сборка для Яндекс Игр

1. **File → Build Profiles** → выберите **Web** (WebGL).
2. Проверьте настройки PluginYG2 (модули, покупки, WebGL-шаблон).
3. Соберите проект, упакуйте папку сборки в `.zip` и загрузите в консоль разработчика Яндекс Игр.

### Баланс игры

Целевые тайминги и формулы прогрессии описаны в [`BALANCE_NOTES.md`](BALANCE_NOTES.md).

### Разработка с помощью ИИ

Проект разрабатывался с использованием ИИ-инструментов (Claude Code) для ревью кода, рефакторинга, исправления ошибок и документации — чтобы быстрее выпускать новые функции. Все решения по дизайну и архитектуре принимались и проверялись автором.

### Лицензия

[MIT](LICENSE)
