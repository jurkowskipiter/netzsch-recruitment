using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Microsoft.AspNetCore.SignalR.Client;

namespace Recruitment.Client;

public partial class MainWindow : Window
{
    public HubConnection connection;
    private string UserName { get; set; } = string.Empty;

    public MainWindow()
    {
        InitializeComponent();

        connection = new HubConnectionBuilder()
            .WithUrl("https://localhost:7181/messagehub")
            .WithAutomaticReconnect()
            .Build();

        connection.Reconnecting += (sender) =>
        {
            this.Dispatcher.Invoke(() =>
            {
                var newMessage = "Attempting to reconect...";
                messages.Items.Add(newMessage);
            });

            return Task.CompletedTask;
        };

        connection.Reconnected += (sender) =>
        {
            this.Dispatcher.Invoke(() =>
            {
                var newMessage = "Reconnected to the server";
                messages.Items.Add(newMessage);
            });

            return Task.CompletedTask;
        };

        connection.Closed += (sender) =>
        {
            this.Dispatcher.Invoke(() =>
            {
                var newMessage = "Connection Closed";
                messages.Items.Add(newMessage);
                openConnection.IsEnabled = true;
                sendMessage.IsEnabled = false;
            });

            return Task.CompletedTask;
        };
    }

    private async void OpenConnection_Click(object sender, RoutedEventArgs e)
    {
        connection.On<string, string>("ReceiveMessage", (user, message) =>
        {
            this.Dispatcher.Invoke(() =>
            {
                var newMessage = $"{user}:{message}";
                messages.Items.Add(newMessage);
            });
        });

        try
        {
            await connection.StartAsync();
            messages.Items.Add("Connection Started");
            openConnection.IsEnabled = false;
            sendMessage.IsEnabled = true;
        }
        catch (Exception ex)
        {
            messages.Items.Add(ex.Message);
        }
    }

    private async void SendMessage_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (UserName == string.Empty)
            {
                UserName = "Default Windows User";
            }

            await connection.InvokeAsync("SendMessage", UserName, messageInput.Text);
        }
        catch (Exception ex)
        {
            messages.Items.Add(ex.Message);
        }
    }

    private void UserName_TextChanged(object sender, TextChangedEventArgs e)
    {
        TextBox textBox = (TextBox)sender;
        UserName = textBox.Text;
    }
}
