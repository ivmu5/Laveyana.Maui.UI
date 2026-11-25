# Подготовка GitHub и NuGet

## GitHub

1. Создать публичный репозиторий `ivmu5/Laveyana.Maui.UI` с веткой `master`. Файлы README, .gitignore, MIT уже в архиве — не создавать конфликтующие шаблоны.
2. Распаковать содержимое архива в корень репозитория и закоммитить.
3. В **Settings → Actions → General** разрешить запуск workflows, в правилах защиты `master` потребовать успешные `Verify / NUnit` и `Verify / Windows WinUI + NuGet smoke` (наименования проверить после первого запуска).
4. Создать Environment `nuget-release`; при желании включить required reviewers.
5. Добавить Actions variable `NUGET_USERNAME` — точное имя пользователя NuGet.org (не секретный API-ключ).

## NuGet Trusted Publishing

Используется такой же принцип, как у `Laveyana.Security`: короткоживущий OIDC, без постоянного API-ключа в secrets. См. [документацию NuGet](https://learn.microsoft.com/en-us/nuget/nuget-org/trusted-publishing).

В NuGet.org в настройках Trusted Publishers создать политику для владельца `ivmu5`, GitHub repository `Laveyana.Maui.UI`, workflow `publish-nuget.yml`, environment `nuget-release`. Убедиться, что политика покрывает **все три** Package ID (`Laveyana.Maui.UI`, `Laveyana.Maui.UI.Windows`, `Laveyana.Maui.UI.Android`). Если NuGet требует индивидуальную регистрацию для каждого идентификатора, зарегистрировать все три. Для новых имён пакетов может потребоваться операция резервирования/первичного выпуска на стороне NuGet.org.

## Автоматический выпуск после PR

После объединения Pull Request с `master` запускается `Verify` для итогового коммита. Если **все** задания Verify успешны, `Automatic Release` проверяет, что коммит всё ещё является последним в `master`, связан с действительно объединённым PR и что у версии из `Directory.Build.props` ещё нет Git-тега.

Затем создаётся GitHub Release с тегом `v.YY.MM.vv`. Только **после** публикации релиза вызывается `Publish NuGet` через `workflow_call`: повторная проверка, сборка, упаковка и выпуск трёх пакетов в NuGet. По умолчанию NuGet использует OIDC Trusted Publishing, без постоянного API-ключа.

**Почему не просто событие `release.published`?** GitHub не запускает вложенные workflows для обычных событий, созданных самим `GITHUB_TOKEN`. Прямой вызов reusable workflow не требует PAT или GitHub App. Ручной опубликованный GitHub Release по-прежнему обрабатывается через `release.published`.

Версию **нужно увеличить в PR** перед очередной публикацией: например, `0.26.11.1` → `0.26.11.2`, а с новым месяцем — `0.26.12.1`. **Без нового тега нового релиза не будет**. Такое ограничение защищает от публикации пакета по любому изменению README.

Глобальная версия `0` отмечается на GitHub как Pre-release. Однако NuGet считает числовые версии `0.*` стабильными; для официальной prerelease-классификации NuGet нужен суффикс, который не входит в согласованный строгий формат.

Перед объединением PR, включающего автоматический выпуск, настройте Environment `nuget-release`, Actions variable `NUGET_USERNAME` и Trusted Publisher для workflow `publish-nuget.yml` на NuGet.org. Можно включить обязательное подтверждение через **Environment → Required reviewers**. Если OIDC настроен неверно, GitHub Release создастся, а публикация NuGet остановится с ошибкой.

## Выпуск (ручной резервный способ)

1. При готовности обновить `LaveyanaVersion` в `Directory.Build.props` по схеме `v.YY.MM.vv` (пример: `0.26.11.1`). Месячный счётчик увеличивается **только при публикации очередного релиза** и сбрасывается на `1` в новом месяце. См. [правила](versioning.ru.md).
2. Дождаться успешного Verify на `master` и **обязательно** запустить Extended Regression вручную перед первым публичным релизом. Оба workflow должны пройти, включая платформенные компиляции.
3. Создать annotated tag `v<version>`, например `v0.26.11.1`, на коммите `master`.
4. Создать и **опубликовать** GitHub Release по этому тегу. При глобальной версии `0` обязательно отметить его как **Pre-release**. Это отметка GitHub; NuGet сам по себе не считает числовую версию `0.*` предварительной.
5. GitHub Actions проверит точный Git-тег, схему `v.YY.MM.vv`, ветку и тесты, затем скомпилирует Windows-пакет, упакует три NuGet и опубликует **сначала основной пакет, затем Windows и Android**. Для месяцев `01`–`09` имя NuGet-пакета проверяется с учётом нормализации ведущего нуля.
6. Проверить появление и статус проверки трёх пакетов в NuGet.org.

## Команды для локальной проверки

```powershell
dotnet --info
dotnet workload install maui-windows
dotnet restore tests/Laveyana.Maui.UI.Tests/Laveyana.Maui.UI.Tests.csproj
dotnet test tests/Laveyana.Maui.UI.Tests/Laveyana.Maui.UI.Tests.csproj -c Release
dotnet build src/Laveyana.Maui.UI.Windows/Laveyana.Maui.UI.Windows.csproj -c Release
dotnet pack src/Laveyana.Maui.UI/Laveyana.Maui.UI.csproj -c Release -o artifacts/nuget
dotnet pack src/Laveyana.Maui.UI.Windows/Laveyana.Maui.UI.Windows.csproj -c Release -o artifacts/nuget
```

На Linux можно запускать тесты основного пакета без Windows-сборки. Apple (iOS/Mac Catalyst) требует macOS и Xcode; проверка сборки по умолчанию раз в месяц и вручную через Extended Regression.

## Политика CI

- PR и `master`: быстрые проверки, **без публикации**.
- 1-го числа каждого месяца: пять прогонов тестов, Android + Apple smoke.
- Ручной регрессионный прогон — перед релизом.
- Нет ежедневных nightly.
- Успешный `Verify` после объединения PR в `master` → `Automatic Release` создаёт GitHub Release новой версии и напрямую вызывает NuGet publisher.
- Опубликованный вручную GitHub Release → OIDC NuGet, только при точном совпадении тега `v<version>` с версией исходников. Месяц в теге всегда двузначный; номер выпуска без ведущих нулей.

Для защиты от ошибочного выпуска разрешения `id-token: write` есть только в job публикации, запущенной в `nuget-release`.

Android-пакет собирается отдельным Linux job. До релиза добавьте новый Package ID в доверенную политику NuGet, иначе публикация платформенного пакета может завершиться ошибкой.
