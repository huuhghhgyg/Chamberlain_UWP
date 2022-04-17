using Microsoft.Toolkit.Uwp.Notifications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Notifications;

namespace BackgroundUpdater
{
    class Notify
    {
        internal class Notification4Tile //针对Tile创建的消息对象模板
        {
            public string title;
            public string description;
            public string displayName;
        }

        internal static class NotificationManager
        {
            // 发送通知参考
            // https://docs.microsoft.com/zh-cn/windows/apps/design/shell/tiles-and-notifications/sending-a-local-tile-notification

            private static void CreateTile(Notification4Tile n, Notification4Tile n1, Notification4Tile n2) //创建中、宽、大尺寸磁贴，大磁贴放2组信息
            {
                var tileContent = new TileContent()
                {
                    Visual = new TileVisual()
                    {
                        TileMedium = new TileBinding()
                        {
                            Branding = TileBranding.Name,
                            DisplayName = n.displayName,
                            Content = new TileBindingContentAdaptive()
                            {
                                Children =
                                {
                                    new AdaptiveText()
                                    {
                                        Text = n.title,
                                        HintWrap = true,
                                        HintMaxLines = 2
                                    },
                                    new AdaptiveText()
                                    {
                                        Text = n.description,
                                        HintStyle = AdaptiveTextStyle.CaptionSubtle
                                    }
                                }
                            }
                        },
                        TileWide = new TileBinding()
                        {
                            Branding = TileBranding.NameAndLogo,
                            Content = new TileBindingContentAdaptive()
                            {
                                Children =
                                {
                                    new AdaptiveText()
                                    {
                                        Text = n.title
                                    },
                                    new AdaptiveText()
                                    {
                                        Text = n.description,
                                        HintStyle = AdaptiveTextStyle.CaptionSubtle
                                    },
                                    new AdaptiveText()
                                    {
                                        Text = n.displayName,
                                        HintStyle = AdaptiveTextStyle.CaptionSubtle
                                    }
                                }
                            }
                        },
                        TileLarge = new TileBinding()
                        {
                            Branding = TileBranding.NameAndLogo,
                            Content = new TileBindingContentAdaptive()
                            {
                                Children =
                                {
                                    new AdaptiveGroup()
                                    {
                                        Children =
                                        {
                                            new AdaptiveSubgroup()
                                            {
                                                Children =
                                                {
                                                    new AdaptiveText()
                                                    {
                                                        Text = n1.title,
                                                        HintWrap = true
                                                    },
                                                    new AdaptiveText()
                                                    {
                                                        Text = n1.description,
                                                        HintStyle = AdaptiveTextStyle.CaptionSubtle
                                                    },
                                                    new AdaptiveText()
                                                    {
                                                        Text = n1.displayName,
                                                        HintStyle = AdaptiveTextStyle.CaptionSubtle
                                                    }
                                                }
                                            }
                                        }
                                    },
                                    new AdaptiveText()
                                    {
                                        Text = ""
                                    },
                                    new AdaptiveGroup()
                                    {
                                        Children =
                                        {
                                            new AdaptiveSubgroup()
                                            {
                                                Children =
                                                {
                                                    new AdaptiveText()
                                                    {
                                                        Text = n2.title,
                                                        HintWrap = true
                                                    },
                                                    new AdaptiveText()
                                                    {
                                                        Text = n2.description,
                                                        HintStyle = AdaptiveTextStyle.CaptionSubtle
                                                    },
                                                    new AdaptiveText()
                                                    {
                                                        Text = n2.displayName,
                                                        HintStyle = AdaptiveTextStyle.CaptionSubtle
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                };

                // Create the tile notification
                var tileNotif = new TileNotification(tileContent.GetXml());

                // And send the notification to the primary tile
                TileUpdateManager.CreateTileUpdaterForApplication().Update(tileNotif);
            }

            private static void CreateTile(Notification4Tile n, Notification4Tile n1) //创建中、宽、大尺寸磁贴，大磁贴放1组信息
            {
                var tileContent = new TileContent()
                {
                    Visual = new TileVisual()
                    {
                        TileMedium = new TileBinding()
                        {
                            Branding = TileBranding.Name,
                            DisplayName = n.displayName,
                            Content = new TileBindingContentAdaptive()
                            {
                                Children =
                                {
                                    new AdaptiveText()
                                    {
                                        Text = n.title,
                                        HintWrap = true,
                                        HintMaxLines = 2
                                    },
                                    new AdaptiveText()
                                    {
                                        Text = n.description,
                                        HintStyle = AdaptiveTextStyle.CaptionSubtle
                                    }
                                }
                            }
                        },
                        TileWide = new TileBinding()
                        {
                            Branding = TileBranding.NameAndLogo,
                            Content = new TileBindingContentAdaptive()
                            {
                                Children =
                                {
                                    new AdaptiveText()
                                    {
                                        Text = n.title
                                    },
                                    new AdaptiveText()
                                    {
                                        Text = n.description,
                                        HintStyle = AdaptiveTextStyle.CaptionSubtle
                                    },
                                    new AdaptiveText()
                                    {
                                        Text = n.displayName,
                                        HintStyle = AdaptiveTextStyle.CaptionSubtle
                                    }
                                }
                            }
                        },
                        TileLarge = new TileBinding()
                        {
                            Branding = TileBranding.NameAndLogo,
                            Content = new TileBindingContentAdaptive()
                            {
                                Children =
                                {
                                    new AdaptiveGroup()
                                    {
                                        Children =
                                        {
                                            new AdaptiveSubgroup()
                                            {
                                                Children =
                                                {
                                                    new AdaptiveText()
                                                    {
                                                        Text = n1.title,
                                                        HintWrap = true
                                                    },
                                                    new AdaptiveText()
                                                    {
                                                        Text = n1.description,
                                                        HintStyle = AdaptiveTextStyle.CaptionSubtle
                                                    },
                                                    new AdaptiveText()
                                                    {
                                                        Text = n1.displayName,
                                                        HintStyle = AdaptiveTextStyle.CaptionSubtle
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                };

                // Create the tile notification
                var tileNotif = new TileNotification(tileContent.GetXml());

                // And send the notification to the primary tile
                TileUpdateManager.CreateTileUpdaterForApplication().Update(tileNotif);
            }

            private static void CreateTile(Notification4Tile n) //创建中、宽尺寸磁贴
            {
                var tileContent = new TileContent()
                {
                    Visual = new TileVisual()
                    {
                        TileMedium = new TileBinding()
                        {
                            Branding = TileBranding.Name,
                            DisplayName = n.displayName,
                            Content = new TileBindingContentAdaptive()
                            {
                                Children =
                                {
                                    new AdaptiveText()
                                    {
                                        Text = n.title,
                                        HintWrap = true,
                                        HintMaxLines = 2
                                    },
                                    new AdaptiveText()
                                    {
                                        Text = n.description,
                                        HintStyle = AdaptiveTextStyle.CaptionSubtle
                                    }
                                }
                            }
                        },
                        TileWide = new TileBinding()
                        {
                            Branding = TileBranding.NameAndLogo,
                            Content = new TileBindingContentAdaptive()
                            {
                                Children =
                                {
                                    new AdaptiveText()
                                    {
                                        Text = n.title
                                    },
                                    new AdaptiveText()
                                    {
                                        Text = n.description,
                                        HintStyle = AdaptiveTextStyle.CaptionSubtle
                                    },
                                    new AdaptiveText()
                                    {
                                        Text = n.displayName,
                                        HintStyle = AdaptiveTextStyle.CaptionSubtle
                                    }
                                }
                            }
                        }
                    }
                };

                // Create the tile notification
                var tileNotif = new TileNotification(tileContent.GetXml());

                // And send the notification to the primary tile
                TileUpdateManager.CreateTileUpdaterForApplication().Update(tileNotif);
            }


            public static void UpdateTileNotification(List<Notification4Tile> list)
            {
                ClearTileNotifications(); //清除消息队列
                TileUpdateManager.CreateTileUpdaterForApplication().EnableNotificationQueue(true); // 开启列表消息循环

                int n = list.Count; // 列表项数
                if (n >= 10)
                {
                    for (int i = 0; i < 5; i++)
                        CreateTile(list[i], list[i], list[i + 5]); // 全部用1+2
                }
                else
                {
                    for (int i = 0, k = 0; i < n && i < 5; i++) //n为0~10
                    {
                        // 可以继续添加
                        k = n - 2 * i; // 双倍剩余项
                        if (k > 0)
                        {
                            if (k == 1) CreateTile(list[i], list[2 * i]); //k=1,只能添加1+1
                            else CreateTile(list[i], list[2 * i], list[2 * i + 1]); //k>1,可以添加1+2
                        }
                        else CreateTile(list[i]); //k<=0,只能添加1+0
                    }
                }
            }

            public static void ClearTileNotifications()
            {
                TileUpdateManager.CreateTileUpdaterForApplication().Clear();
            }

            public static void SendNotification(string title, string content)
            {
                // 触发通知
                var toastContent = new ToastContent()
                {
                    Visual = new ToastVisual()
                    {
                        BindingGeneric = new ToastBindingGeneric()
                        {
                            Children =
                            {
                                new AdaptiveText()
                                {
                                    Text = title
                                },
                                new AdaptiveText()
                                {
                                    Text = content
                                }
                            }
                        }
                    }
                };

                // Create the toast notification
                var toastNotif = new ToastNotification(toastContent.GetXml());

                // And send the notification
                ToastNotificationManager.CreateToastNotifier().Show(toastNotif);
            }

            public static void SendNotification_ReminderCheck(string title, string content, TimeSpan remain_timespan)
            {
                new ToastContentBuilder()
                .AddText(title)
                .AddText(content)
                // Buttons
                .AddButton(new ToastButton()
                    .SetContent("标记完成")
                    .AddArgument("action", "Check")
                    .SetBackgroundActivation())
                .AddButton(new ToastButton()
                    .SetContent("稍后提醒")
                    .AddArgument("action", "None")
                    .SetBackgroundActivation())
                .Show(toast =>
                {
                    toast.ExpirationTime = DateTime.Now + remain_timespan;
                });
            }
        }

    }
}
