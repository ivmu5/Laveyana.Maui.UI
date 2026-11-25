# Платформенные возможности

| Возможность | Windows | Android | iOS / Mac Catalyst |
|---|---|---|---|
| Темы, контролы, анимации | core | core | core |
| Запрет автоотключения экрана | MAUI Essentials при поддержке платформой | MAUI Essentials | MAUI Essentials при поддержке платформой |
| Управление заголовком окна | Windows | — | — |
| Показ Activity поверх блокировки | — | Android | — |
| Системные панели Android | — | Android | — |
| Состояние визуальных обновлений | UiVisibilityState + события окна | UiAndroidScreenMonitor | UiVisibilityState + события окна |

Android: создайте один UiAndroidScreenMonitor для Activity; OnStart → Start(), OnResume → SetResumed(true), OnPause → SetResumed(false), OnStop → Stop(), OnDestroy → Dispose(). Подпишитесь на State.RenderAvailabilityChanged и проверяйте State.CanRender при запуске отрисовки. Вызовы на UI-потоке.

ShowWhenLocked не отключает аутентификацию, не запускает приложение поверх других из фона и не даёт права доступа к защищённым данным. По умолчанию выключено; включение — явное решение приложения и пользователя. Не отображайте чувствительные данные поверх блокировки. На iOS произвольный UI приложения поверх экрана блокировки недоступен; системные уведомления, Live Activities и медиаконтролы имеют отдельные API и ограничения, не реализованные этим toolkit.

UiScreen.SetKeepAwakeAsync действует на приложение глобально: один владелец отвечает за включение и восстановление состояния. Это не аналог ShowWhenLocked. UiVisibilityState не подписывается на платформенные события автоматически.
