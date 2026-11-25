# Сбой автоматического релиза 23.11.2026 — 45185999899

После успешного `Verify` и проверки объединённого PR задание `Create GitHub Release` завершилось ошибкой `fatal: not a git repository`: в данном job не выполнялся `actions/checkout`, поэтому `gh release create` не мог определить репозиторий.

Исправление: перед созданием релиза добавлен `actions/checkout@v4` с точным проверенным SHA, а также `GH_REPO` с именем GitHub-репозитория. В `tools/check_workflows.py` добавлена проверка обязательного checkout перед `gh release create`, в регрессионных тестах — соответствующий случай.

Release `v0.26.11.1` и Git-тег на момент исправления отсутствовали, поэтому после объединения исправления можно повторить автоматическую публикацию той же версии. Проверки в PR и после объединения обязательны.

---

# Автоматическая публикация — исправление от 23.11.2026

Запуск `Automatic Release` [45183107051](https://github.com/ivmu5/Laveyana.Maui.UI/actions/runs/45183107051) завершился на `actions/checkout`: `fatal: invalid refspec '+refs/heads/${ github.event.workflow_run.head_sha }*'`.

Причина: в `automatic-release.yml` и `publish-nuget.yml` вместо обязательного синтаксиса выражений GitHub Actions `${{ github.* }}` ошибочно присутствовал текст `${ github.* }`, который не интерпретируется движком. Исправлены все такие вхождения в двух workflow.

В `tools/check_workflows.py` добавлена отдельная проверка неправильных GitHub-выражений, а в `tools/test_workflow_syntax.py` — регрессионные тесты. Ранее YAML валидировался, но эта ошибка не обнаруживалась.

**После слияния PR исправления:** заново запустятся Verify на `master`, затем Automatic Release. До исправления релиз не был создан; пакет NuGet не публиковался.

---

# Дополнение: версионирование и повторный Verify — 23.11.2026

На PR #1 повторный Verify подтвердил Windows WinUI + NuGet smoke и NUnit под Ubuntu, но Windows-сборка провалилась на выводе результата TRX: Python выдавал `UnicodeEncodeError: 'charmap'` при печати кириллицы. В `tools/check_trx.py` включён UTF-8 для stdout/stderr. Требуется следующий CI-прогон.

Версия изменена на `0.26.11.1` согласно схеме `v.YY.MM.vv`. Для выпуска добавлена проверка четырёх числовых компонентов и нормализация месяца при определении имени NuGet-пакета. Строгая проверка версии доступна как `python tools/check_version.py`.

---

# Проверка GitHub Actions — 23.11.2026

Запуск Verify #1 (45168313025) выявил две независимые проблемы подготовки CI:

1. Тесты NUnit наследовали `GenerateDocumentationFile=true` вместе с `TreatWarningsAsErrors=true`. Это порождало `CS1591` у тестовых классов и методов до запуска самих тестов. Для тестовых проектов XML-документация отключена, для публикуемых библиотек сохранена.
2. Smoke-проект использовал глобальное `-p:TargetFramework=...`, которое передавалось проектам по `ProjectReference` и могло менять целевой TFM переносимой `Laveyana.Maui.UI`. При Windows-сборке это привело к `MSB4086` (неопределённый `TargetPlatformMinVersion`). Введено собственное свойство `SmokeTargetFramework`, а также указан Windows minimum для самого smoke-проекта.

Прежний сбой не означает, что NUnit-тесты выполнялись. После исправлений требуется новый прогон Verify и проверка release workflow.

---

# Исправления после компиляции: 0.1.0-alpha.3

Дата: 23 ноября 2026 года.

На основании нового отчёта Visual Studio были найдены две ошибки `CS0121` в `UiAdaptiveGrid.cs:92-93`:
`Grid.SetColumn` и `Grid.SetRow` одновременно имеют перегрузки для `BindableObject` и `IView`.
Экземпляр `Microsoft.Maui.Controls.View` соответствует обоим типам, поэтому компилятор не может выбрать однозначную перегрузку.

Исправление: в вызовах использовать `(BindableObject)` и явный тип `Grid`, например
`Grid.SetColumn((BindableObject)_cards[i], i % next)`.
Аналогичным образом закреплён выбор перегрузок в `UiFluentExtensions`, `UiNumberInput`, `Ui`, `UiTabHost`.

`CS0006` для `Laveyana.Maui.UI.Windows` — следствие отсутствующей DLL `Laveyana.Maui.UI.dll`, поскольку основная библиотека не собралась. Если ошибка останется после успешной сборки основной библиотеки, потребуется отдельная проверка пути сборки и ProjectReference.

**Результат:** исправления применены и исходники проверены статически. `dotnet build` и `dotnet test` в этой среде не запускались: SDK .NET отсутствует. Перед публикацией пакета необходим успешный прогон CI.

---

# Результат повторной проверки: 0.1.0-alpha.2

Дата: 23 ноября 2026 года.

## Исходные данные

Проверен исходный архив `0.1.0-alpha.1` и предоставленный отчёт компиляции Visual Studio (89 диагностик: 20 ошибок, 69 предупреждений).

| Диагностика | Число записей | Причина | Изменение |
|---|---:|---|---|
| `CS0119` | 1 | `Ui.Button` закрывал имя типа при обращении к `Button.TextColorProperty` | Явно указано `Microsoft.Maui.Controls.Button` |
| `CS0311` | 1 | Пространство имён `Laveyana.Maui.UI.Layout` затеняло тип MAUI `Layout` | Указан `Microsoft.Maui.Controls.Layout` |
| `CS1501` | 3 | В `UiNumberInput` использовались несуществующие перегрузки `Grid.Add(child, index)` | Назначение колонки через `Grid.SetColumn`, добавление в `Children` |
| `CS0104` | 15 | Конфликты MAUI/WinUI для `Image`, `Stretch`, `KeyboardAccelerator`, `VerticalAlignment`, `HorizontalAlignment` | Введены однозначные WinUI-алиасы |
| `CS1591` | 69 | Недокументированные публичные методы/свойства/enum-значения | Добавлены XML-описания, предупреждения не подавлены |

## Дополнительные исправления

- Устранено пустое пространство при `Ui.Number(..., showSlider: false)`.
- Исправлены сохранение пользовательской палитры при переключении пресета и повторная синхронизация редактора цветов.
- `UiAdaptiveGrid` сохраняет исходные ограничения высоты карточек, проверяет дублированные экземпляры и недопустимую высоту.
- `UiPaletteCatalog.Get(Custom)` больше не выдаёт неявно Laveyana; требуются пользовательские значения через `UiThemeSettings`.
- Заголовок Windows снимает подписки на клавиатурные обработчики и событие изменения размеров содержимого.
- В `Directory.Build.props` включён `TreatWarningsAsErrors`.
- В тесты добавлены граничные случаи нормализации, палитр, контраста и виртуализации.
- Быстрые и релизные проверки теперь удостоверяются, что TRX содержит реальные прошедшие тесты.
- Добавлена компиляция Windows smoke-consumer, использующего обе сборки.

## Что удалось подтвердить здесь

- Синтаксическая корректность XML всех `csproj`, `Directory.Build.props` и `slnx`.
- Корректность структуры YAML всех трёх workflow.
- Наличие XML summary для всех явно публичных деклараций в исходниках библиотеки; не осталось известных неоднозначных обращений из отчёта.
- Синтетическая проверка парсера TRX.
- Согласованность версии NuGet и структуры зависимостей (Windows → основной пакет).

**Важно:** это структурные/статические проверки, а не успешно выполненная сборка. В этой среде отсутствует `dotnet` и сетевой доступ к загрузке SDK. Запустить `dotnet restore`, `dotnet build`, `dotnet test`, `dotnet pack`, Windows UI и Android/iOS smoke пока **не удалось**. Повторная компиляция на .NET 10 и GitHub Actions обязательна перед NuGet Release. Исправления известных ошибок требуют подтверждения компилятором: нельзя заявлять, что ошибок точно не осталось.

## Команды для полноценной проверки на Windows

```powershell
dotnet --info
dotnet workload install maui-windows
dotnet restore tests/Laveyana.Maui.UI.Tests/Laveyana.Maui.UI.Tests.csproj
dotnet test tests/Laveyana.Maui.UI.Tests/Laveyana.Maui.UI.Tests.csproj -c Release --logger "trx;LogFileName=local.trx" --results-directory artifacts/trx
python tools/check_trx.py artifacts/trx/local.trx
dotnet restore src/Laveyana.Maui.UI.Windows/Laveyana.Maui.UI.Windows.csproj
dotnet build src/Laveyana.Maui.UI.Windows/Laveyana.Maui.UI.Windows.csproj -c Release
dotnet restore tests/Laveyana.Maui.UI.Smoke/Laveyana.Maui.UI.Smoke.csproj -p:SmokeTargetFramework=net10.0-windows10.0.19041.0
dotnet build tests/Laveyana.Maui.UI.Smoke/Laveyana.Maui.UI.Smoke.csproj -c Release -p:SmokeTargetFramework=net10.0-windows10.0.19041.0
dotnet pack src/Laveyana.Maui.UI/Laveyana.Maui.UI.csproj -c Release -o artifacts/nuget
dotnet pack src/Laveyana.Maui.UI.Windows/Laveyana.Maui.UI.Windows.csproj -c Release -o artifacts/nuget
```

## Риски для ручных UI-тестов

1. Стандартные и нестандартные DPI, прозрачный заголовок Windows и интерактивные области перетаскивания.
2. Переход между F11, Esc, maximize и minimize и восстановление прямоугольника окна.
3. Взаимодействия `GraphicsView` (HSV picker) и двойное касание слайдера на Android/iOS.
4. Отмена анимаций, сокрытие экранов и частота кадров на слабых устройствах.
5. Повторное использование строк виртуализированного списка при быстрой прокрутке и замене моделей.
6. Миграция Preferences и представлений приложений-потребителей без потери поведения приложений.

## Release-gate

**До первого публичного NuGet Release:** должны успешно отработать `Verify` на GitHub, расширенный регрессионный прогон (включая Android и Apple build) и ручной smoke-тест Windows/Android. До этого оба пакета считаются предрелизными исходниками без подтверждённой работоспособности.
