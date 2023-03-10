﻿using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Data;

namespace PrimalEditor.Utilities;

public enum MessageType
{
    Info = 0x01,
    Warning = 0x02,
    Error = 0x04,
}
public class LogMessage
{
    public DateTime Time { get; }
    public MessageType Type { get; }
    public string Message { get; }

    public string File { get; }

    public string Caller { get; }
    public int Line { get; }

    public string Metadata => $"{File}: {Caller} ({Line})";

    public LogMessage(MessageType type, string message, string file, string caller, int line)
    {
        Time = DateTime.Now;
        Type = type;
        Message = message;
        File = Path.GetFileName(file);
        Caller = caller;
        Line = line;
    }
}

public static class Logger
{
    private static int _messageFilter = (int)(MessageType.Info | MessageType.Warning | MessageType.Error);

    private static readonly ObservableCollection<LogMessage> _messages = new();

    public static ReadOnlyObservableCollection<LogMessage> Messages { get; } =
        new ReadOnlyObservableCollection<LogMessage>(_messages);

    public static CollectionViewSource FilteredMessages { get; } = new()
    {
        Source = Messages
    };

    public static async void Log(MessageType messageType, string message,
        [CallerFilePath] string file = "",
        [CallerMemberName] string caller = "",
        [CallerLineNumber] int line = 0)
    {
        await Application.Current.Dispatcher.BeginInvoke(new Action(() =>
        {
            _messages.Add(new LogMessage(messageType, message, file, caller, line));
        }));
    }

    public static async void Clear()
    {
        await Application.Current.Dispatcher.BeginInvoke(new Action(() =>
        {
            _messages.Clear();
        }));
    }

    public static void SetMessageFilter(int mask)
    {
        _messageFilter = mask;

        FilteredMessages.View.Refresh();
    }

    static Logger()
    {
        FilteredMessages.Filter += (s, e) =>
        {
            var type = (int)((LogMessage)e.Item).Type;

            e.Accepted = (type & _messageFilter) != 0;
        };
    }
}