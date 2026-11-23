# Подготовка GitHub и NuGet

## GitHub

1. Создать публичный репозиторий `ivmu5/Laveyana.Maui.UI` с веткой `master`. Файлы README, .gitignore, MIT уже в архиве — не создавать конфликтующие шаблоны.
2. Распаковать содержимое архива в корень репозитория и закоммитить.
3. В **Settings → Actions → General** разрешить запуск workflows, в правилах защиты `master` потребовать успешные `Verify / NUnit` и `Verify / Windows WinUI + NuGet smoke` (наименования проверить после первого запуска).
4. Создать Environment `nuget-release`; при желании включить required reviewers.
5. Добавить Actions variable `NUGET_USERNAME` — точное имя пользователя NuGet.org (не секретный API-ключ).

## NuGet Trusted Publishing

Используется такой же принцип, как у `Laveyana.Security`: короткоживущий OIDC, без постоянного API-ключа в secrets. См. [документацию NuGet](https://learn.microsoft.com/en-us/nuget/nuget-org/trusted-publishing).

В NuGet.org в настройках Trusted Publishers создать политику для владельца `ivmu5`, GitHub repository `Laveyana.Maui.UI`, workflow `publish-nuget.yml`, environment `nuget-release`. Убедиться, что политика покрывает **оба** Package ID (`Laveyana.Maui.UI` и `Laveyana.Maui.UI.Windows`). Если NuGet требует индивидуальную регистрацию для каждого идентификатора, зарегистрировать оба. Для новой пары имён пакетов может потребоваться операция резервирования/первичного выпуска на стороне NuGet.org.

## Выпуск

1. При готовности обновить `LaveyanaVersion` в `Directory.Build.props`.
2. Дождаться успешного Verify на `master` и **обязательно** запустить Extended Regression вручную перед первым публичным релизом. Оба workflow должны пройти, включая платформенные компиляции.
3. Создать annotated tag `v<version>`, например `v0.1.0-alpha.3`, на коммите `master`.
4. Создать и **опубликовать** GitHub Release по этому тегу, пометив prerelease для версий с `-alpha`, `-beta`, `-rc`.
5. GitHub Actions проверит версию, ветку, тесты, скомпилирует Windows-пакет, упакует оба NuGet и опубликует **сначала основной пакет, затем Windows**.
6. Проверить появление и статус проверки двух пакетов в NuGet.org.

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
- Опубликованный GitHub Release → OIDC NuGet, только при совпадении `v<version>` с версией исходников.

Для защиты от ошибочного выпуска разрешения `id-token: write` есть только в job публикации, запущенной в `nuget-release`.
