using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using LiteDB;
using QRCoder;
using System.IO;
using System.Text;
using AppLauncher.Classes.Core_Classes;
using AppLauncher.Classes;
using System.Windows.Media.Animation;
using System.Threading.Tasks;
using MessageBox = AppLauncher.Classes.MessageBox;

namespace AppLauncher.Views.Pages
{
    public class ThemeColorsTaskManager
    {
        public string PageBackground { get; set; }
        public string PrimaryColor { get; set; }
        public string PrimaryHover { get; set; }
        public string SecondaryColor { get; set; }

        public string AccentColor { get; set; }
        public string DangerColor { get; set; }
        public string WarningColor { get; set; }
        public string CardBackground { get; set; }

        public string CardBorder { get; set; }
        public string TextPrimary { get; set; }
        public string TextSecondary { get; set; }
        public string TextMuted { get; set; }

        public string SidebarBackground { get; set; }
        public string SidebarBorder { get; set; }
        public string SidebarText { get; set; }
        public string HoverBackground { get; set; }

        public string SelectedBackground { get; set; }
        public string ProgressBackground { get; set; }
        public string InputBackground { get; set; }

        public string InputBorder { get; set; }
        public string InputFocusBorder { get; set; }
        public string CaretBrushTextBoxes { get; set; }


        public string ComboBoxBackground { get; set; }
        public string ComboBoxBorder { get; set; }
        public string ComboBoxText { get; set; }
        public string ComboBoxHoverBackground { get; set; }
        public string ComboBoxFocusBorder { get; set; }
        public string ComboBoxDisabledBackground { get; set; }
        public string ComboBoxDisabledText { get; set; }
        public string ComboBoxArrow { get; set; }
        public string ComboBoxDropdownBackground { get; set; }
        public string ComboBoxItemHoverBackground { get; set; }
        public string ComboBoxItemSelectedBackground { get; set; }
    }
    public partial class TaskManagerPage : Page
    {
        private LiteDatabase _database;
        private ILiteCollection<ProjectModel> _projectsCollection;
        private ILiteCollection<TaskModel> _tasksCollection;
        private ObservableCollection<ProjectModel> _projects;
        private string _currentFilter = "all";
        private int? _currentTaskId = null;
        private int? _currentParentTaskId = null;
        private int? _currentProjectId = null;
        private ApplyThemes apply;
        private Geometry menuCollapsed = Geometry.Parse(
    "M 7 11 A 3 3 0 0 0 7 17 A 3 3 0 0 0 7 11 Z " +
    "M 16 12 A 2.0002 2.0002 0 1 0 16 16 L 42 16 A 2.0002 2.0002 0 1 0 42 12 L 16 12 Z " +
    "M 7 21 A 3 3 0 0 0 7 27 A 3 3 0 0 0 7 21 Z " +
    "M 16 22 A 2.0002 2.0002 0 1 0 16 26 L 42 26 A 2.0002 2.0002 0 1 0 42 22 L 16 22 Z " +
    "M 7 31 A 3 3 0 0 0 7 37 A 3 3 0 0 0 7 31 Z " +
    "M 16 32 A 2.0002 2.0002 0 1 0 16 36 L 42 36 A 2.0002 2.0002 0 1 0 42 32 L 16 32 Z"
);
        public TaskManagerPage(ApplyThemes aply)
        {
            InitializeComponent();
            InitializeDatabase();
            apply = aply;
            Loaded += TaskManagerPage_Loaded;
            // Delay initialization until loaded
            this.Loaded += (s, e) => InitializeData();
        }

        private void TaskManagerPage_Loaded(object sender, RoutedEventArgs e)
        {
            ApplyCustomTheme(new ThemeColorsTaskManager
            {
                // Background Colors
                PageBackground = apply.BackgroundPrimary,
                PrimaryColor = apply.BackgroundPrimary,
                PrimaryHover = apply.BackgroundSecundary,
                SecondaryColor = apply.BackgroundLightPart,

                // Accent and Status Colors
                AccentColor = apply.MenuIconColor,
                DangerColor = "#FFF63B4F",
                WarningColor = "#FFF6D63B",
                CardBackground = apply.BackgroundSecundary,

                // Border and Text Colors
                CardBorder = apply.BackgroundLightPart,
                TextPrimary = apply.ForegroundPrimary,
                TextSecondary = apply.ForegroundSecundary,
                TextMuted = apply.ForegroundStatus,

                // Sidebar Colors
                SidebarBackground = apply.BackgroundSecundary,
                SidebarBorder = apply.BackgroundLightPart,
                SidebarText = apply.ForegroundPrimary,
                HoverBackground = apply.BackgroundSelectedTile,

                // Input and Progress Colors
                SelectedBackground = apply.BackgroundLightPart,
                ProgressBackground = apply.BackgroundLightPart,
                InputBackground = apply.BackgroundPrimary,

                // Border Colors
                InputBorder = apply.MenuIconColor,
                InputFocusBorder = apply.MenuIconColor,
                CaretBrushTextBoxes = apply.MenuIconColor
            });
        }
        private void ApplyCustomTheme(ThemeColorsTaskManager theme)
        {
            // Tab Colors
            SetBrushColor("PageBackground", theme.PageBackground);
            SetBrushColor("PrimaryColor", theme.PrimaryColor);
            SetBrushColor("PrimaryHover", theme.PrimaryHover);
            SetBrushColor("SecondaryColor", theme.SecondaryColor);

            // Star Colors
            SetBrushColor("AccentColor", theme.AccentColor);
            SetBrushColor("DangerColor", theme.DangerColor);
            SetBrushColor("WarningColor", theme.WarningColor);
            SetBrushColor("CardBackground", theme.CardBackground);

            // Bookmark Colors
            SetBrushColor("CardBorder", theme.CardBorder);
            SetBrushColor("TextPrimary", theme.TextPrimary);
            SetBrushColor("TextSecondary", theme.TextSecondary);
            SetBrushColor("TextMuted", theme.TextMuted);

            // Rate Colors
            SetBrushColor("SidebarBackground", theme.SidebarBackground);
            SetBrushColor("SidebarBorder", theme.SidebarBorder);
            SetBrushColor("SidebarText", theme.SidebarText);
            SetBrushColor("HoverBackground", theme.HoverBackground);

            // Text Colors
            SetBrushColor("SelectedBackground", theme.SelectedBackground);
            SetBrushColor("ProgressBackground", theme.ProgressBackground);
            SetBrushColor("InputBackground", theme.InputBackground);

            // Button Colors
            SetBrushColor("InputBorder", theme.InputBorder);
            SetBrushColor("InputFocusBorder", theme.InputFocusBorder);
            SetBrushColor("CaretBrushTextBoxes", theme.CaretBrushTextBoxes);
        }
        private void SetBrushColor(string resourceKey, string colorHex)
        {
            try
            {
                Color color = (Color)ColorConverter.ConvertFromString(colorHex);

                if (this.Resources.Contains(resourceKey))
                {
                    var obj = this.Resources[resourceKey];
                    if (obj is SolidColorBrush scb && !scb.IsFrozen)
                    {
                        scb.Color = color;
                    }
                    else
                    {
                        this.Resources[resourceKey] = new SolidColorBrush(color);
                    }
                }
                else
                {
                    this.Resources.Add(resourceKey, new SolidColorBrush(color));
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error setting color for {resourceKey}: {ex.Message}");
            }
        }
        #region Database Initialization

        private void InitializeDatabase()
        {
            try
            {
                _database = DatabaseManager.GetTaskDatabase();
                _projectsCollection = _database.GetCollection<ProjectModel>("projects");
                _tasksCollection = _database.GetCollection<TaskModel>("tasks");

                // Create indexes
                _projectsCollection.EnsureIndex(x => x.Id);
                _tasksCollection.EnsureIndex(x => x.Id);
                _tasksCollection.EnsureIndex(x => x.ProjectId);
                _tasksCollection.EnsureIndex(x => x.ParentTaskId);
                _tasksCollection.EnsureIndex(x => x.Status);
                _tasksCollection.EnsureIndex(x => x.DueDate);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Database initialization error: {ex.Message}", "Error", MessageBox.MessageBoxButton.OK, MessageBox.MessageBoxIcon.Error);
            }
        }

        #endregion

        #region Initialization

        private void InitializeData()
        {
            // Initialize date
            if (TodayDateText != null)
                TodayDateText.Text = DateTime.Now.ToString("dddd, MMMM dd, yyyy");

            // Initialize status options
            TaskStatusInput.Items.Add("Pending");
            TaskStatusInput.Items.Add("In Progress");
            TaskStatusInput.Items.Add("Completed");
            TaskStatusInput.Items.Add("On Hold");
            TaskStatusInput.SelectedIndex = 0;

            // Initialize priority options
            TaskPriorityInput.Items.Add("Low");
            TaskPriorityInput.Items.Add("Medium");
            TaskPriorityInput.Items.Add("High");
            TaskPriorityInput.Items.Add("Critical");
            TaskPriorityInput.SelectedIndex = 1;

            // Initialize project colors
            ProjectColorInput.Items.Add("Blue");
            ProjectColorInput.Items.Add("Green");
            ProjectColorInput.Items.Add("Purple");
            ProjectColorInput.Items.Add("Orange");
            ProjectColorInput.Items.Add("Red");
            ProjectColorInput.Items.Add("Pink");
            ProjectColorInput.SelectedIndex = 0;

            // Load projects and tasks
            LoadProjects();
            CreateDefaultProjectIfNeeded();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (TasksContainer != null)
            {
                RefreshTasksView();
            }
        }

        private void CreateDefaultProjectIfNeeded()
        {
            if (_projectsCollection.Count() == 0)
            {
                var defaultProject = new ProjectModel
                {
                    Name = "Personal",
                    Description = "Personal tasks and projects",
                    Color = "Blue",
                    CreatedDate = DateTime.Now
                };
                _projectsCollection.Insert(defaultProject);
                LoadProjects();
            }
        }

        #endregion

        #region Data Loading

        private void LoadProjects()
        {
            _projects = new ObservableCollection<ProjectModel>(
                _projectsCollection.FindAll().OrderBy(p => p.Name)
            );

            // Update progress for each project
            foreach (var project in _projects)
            {
                UpdateProjectProgress(project);
            }

            ProjectsList.ItemsSource = _projects;
            TaskProjectInput.ItemsSource = _projects;

            if (_projects.Count > 0)
            {
                TaskProjectInput.SelectedIndex = 0;
            }
        }

        private void UpdateProjectProgress(ProjectModel project)
        {
            var projectTasks = _tasksCollection.Find(t => t.ProjectId == project.Id && t.ParentTaskId == null).ToList();
            var completedTasks = projectTasks.Count(t => t.Status == "Completed");
            var totalTasks = projectTasks.Count;

            project.CompletedTasks = completedTasks;
            project.TotalTasks = totalTasks;
            project.Progress = totalTasks > 0 ? (completedTasks * 100.0 / totalTasks) : 0;
            project.ProgressText = $"{completedTasks}/{totalTasks} tasks";

            _projectsCollection.Update(project);
        }

        private void ShowEmptyState()
        {
            var emptyPanel = new StackPanel
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 100, 0, 0)
            };

            var emptyIcon = new TextBlock
            {
                Text = "📋",
                FontSize = 64,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 0, 0, 16)
            };

            var emptyText = new TextBlock
            {
                Text = "No tasks found",
                FontSize = 20,
                FontWeight = FontWeights.SemiBold,
                Foreground = (SolidColorBrush)FindResource("TextSecondary"),
                HorizontalAlignment = HorizontalAlignment.Center
            };

            var emptySubtext = new TextBlock
            {
                Text = "Create a new task to get started",
                FontSize = 14,
                Foreground = (SolidColorBrush)FindResource("TextMuted"),
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 8, 0, 0)
            };

            emptyPanel.Children.Add(emptyIcon);
            emptyPanel.Children.Add(emptyText);
            emptyPanel.Children.Add(emptySubtext);

            TasksContainer.Children.Add(emptyPanel);
        }

        #endregion

        #region Task Card Creation

        private Border CreateTaskCard(TaskModel task)
        {
            var card = new Border
            {
                Style = (Style)FindResource("TaskCard"),
                Tag = task
            };

            var mainGrid = new Grid();
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            // Header section
            var headerGrid = new Grid { Margin = new Thickness(0, 0, 0, 12) };
            headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            var titlePanel = new StackPanel();

            var titleText = new TextBlock
            {
                Text = task.Title,
                FontSize = 18,
                FontWeight = FontWeights.SemiBold,
                Foreground = (SolidColorBrush)FindResource("TextPrimary"),
                TextWrapping = TextWrapping.Wrap
            };

            var metaPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(0, 6, 0, 0)
            };

            // Project badge
            var project = _projectsCollection.FindById(task.ProjectId);
            if (project != null)
            {
                var projectBadge = new Border
                {
                    Background = GetProjectColorBrush(project.Color),
                    CornerRadius = new CornerRadius(4),
                    Padding = new Thickness(8, 4, 8, 4),
                    Margin = new Thickness(0, 0, 8, 0)
                };

                var projectText = new TextBlock
                {
                    Text = project.Name,
                    FontSize = 11,
                    FontWeight = FontWeights.Medium,
                    Foreground = Brushes.White
                };

                projectBadge.Child = projectText;
                metaPanel.Children.Add(projectBadge);
            }

            // Priority badge
            var priorityBadge = new Border
            {
                Background = GetPriorityColorBrush(task.Priority),
                CornerRadius = new CornerRadius(4),
                Padding = new Thickness(8, 4, 8, 4),
                Margin = new Thickness(0, 0, 8, 0)
            };

            var priorityText = new TextBlock
            {
                Text = task.Priority,
                FontSize = 11,
                FontWeight = FontWeights.Medium,
                Foreground = Brushes.White
            };

            priorityBadge.Child = priorityText;
            metaPanel.Children.Add(priorityBadge);

            // Due date
            if (task.DueDate.HasValue)
            {
                var dueDateText = new TextBlock
                {
                    Text = $"📅 {task.DueDate.Value:MMM dd}",
                    FontSize = 12,
                    Foreground = (SolidColorBrush)FindResource("TextSecondary"),
                    VerticalAlignment = VerticalAlignment.Center
                };

                if (task.DueDate.Value.Date < DateTime.Today && task.Status != "Completed")
                {
                    dueDateText.Foreground = (SolidColorBrush)FindResource("DangerColor");
                    dueDateText.FontWeight = FontWeights.SemiBold;
                }

                metaPanel.Children.Add(dueDateText);
            }

            titlePanel.Children.Add(titleText);
            titlePanel.Children.Add(metaPanel);

            // Action buttons
            var actionsPanel = new StackPanel { Orientation = Orientation.Horizontal };

            var statusButton = new Button
            {
                Style = (Style)FindResource("IconButton"),
                Content = GetStatusIcon(task.Status),
                ToolTip = task.Status,
                Tag = task
            };
            statusButton.Click += ToggleTaskStatus_Click;

            var qrButton = new Button
            {
                Style = (Style)FindResource("IconButton"),
                Content = "📱",
                ToolTip = "Generate QR Code",
                Tag = task
            };
            qrButton.Click += ShowQRCode_Click;

            var editButton = new Button
            {
                Style = (Style)FindResource("IconButton"),
                Content = "✏️",
                ToolTip = "Edit Task",
                Tag = task
            };
            editButton.Click += EditTask_Click;

            var deleteButton = new Button
            {
                Style = (Style)FindResource("IconButton"),
                Content = "🗑️",
                ToolTip = "Delete Task",
                Tag = task
            };
            deleteButton.Click += DeleteTask_Click;

            actionsPanel.Children.Add(statusButton);
            actionsPanel.Children.Add(qrButton);
            actionsPanel.Children.Add(editButton);
            actionsPanel.Children.Add(deleteButton);

            Grid.SetColumn(titlePanel, 0);
            Grid.SetColumn(actionsPanel, 1);
            headerGrid.Children.Add(titlePanel);
            headerGrid.Children.Add(actionsPanel);

            Grid.SetRow(headerGrid, 0);
            mainGrid.Children.Add(headerGrid);

            // Description
            if (!string.IsNullOrWhiteSpace(task.Description))
            {
                var descText = new TextBlock
                {
                    Text = task.Description,
                    FontSize = 14,
                    Foreground = (SolidColorBrush)FindResource("TextSecondary"),
                    TextWrapping = TextWrapping.Wrap,
                    Margin = new Thickness(0, 0, 0, 12)
                };

                Grid.SetRow(descText, 1);
                mainGrid.Children.Add(descText);
            }

            // Progress bar and tags
            var footerPanel = new StackPanel();

            // Progress bar
            if (task.SubTasks != null && task.SubTasks.Count > 0)
            {
                var progressPanel = new StackPanel { Margin = new Thickness(0, 0, 0, 12) };

                var progressHeader = new Grid { Margin = new Thickness(0, 0, 0, 4) };
                progressHeader.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                progressHeader.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

                var progressLabel = new TextBlock
                {
                    Text = "Subtasks",
                    FontSize = 12,
                    FontWeight = FontWeights.SemiBold,
                    Foreground = (SolidColorBrush)FindResource("TextSecondary")
                };

                var completedSubtasks = task.SubTasks.Count(st => st.Status == "Completed");
                var totalSubtasks = task.SubTasks.Count;
                var progressPercentage = (double)completedSubtasks / totalSubtasks * 100;

                var progressText = new TextBlock
                {
                    Text = $"{completedSubtasks}/{totalSubtasks} ({progressPercentage:F0}%)",
                    FontSize = 12,
                    FontWeight = FontWeights.SemiBold,
                    Foreground = (SolidColorBrush)FindResource("TextPrimary")
                };

                Grid.SetColumn(progressLabel, 0);
                Grid.SetColumn(progressText, 1);
                progressHeader.Children.Add(progressLabel);
                progressHeader.Children.Add(progressText);

                var progressBarBorder = new Border
                {
                    Background = (SolidColorBrush)FindResource("ProgressBackground"),
                    Height = 8,
                    CornerRadius = new CornerRadius(4)
                };

                var progressBarFill = new Border
                {
                    Background = (SolidColorBrush)FindResource("AccentColor"),
                    Height = 8,
                    CornerRadius = new CornerRadius(4),
                    HorizontalAlignment = HorizontalAlignment.Left,
                    Width = progressBarBorder.ActualWidth * (progressPercentage / 100)
                };

                progressBarBorder.Child = progressBarFill;
                progressBarBorder.SizeChanged += (s, e) =>
                {
                    progressBarFill.Width = e.NewSize.Width * (progressPercentage / 100);
                };

                progressPanel.Children.Add(progressHeader);
                progressPanel.Children.Add(progressBarBorder);

                footerPanel.Children.Add(progressPanel);

                // Subtasks expander
                var expander = new Expander
                {
                    Header = "Show Subtasks",
                    Margin = new Thickness(0, 8, 0, 0),
                    FontSize = 13,
                    FontWeight = FontWeights.SemiBold,
                    Foreground = (SolidColorBrush)FindResource("PrimaryColor")
                };

                var subtasksPanel = new StackPanel { Margin = new Thickness(0, 8, 0, 0) };

                foreach (var subtask in task.SubTasks.OrderBy(st => st.CreatedDate))
                {
                    subtasksPanel.Children.Add(CreateSubtaskCard(subtask, task));
                }

                var addSubtaskButton = new Button
                {
                    Content = "➕ Add Subtask",
                    Style = (Style)FindResource("ModernButton"),
                    Background = (SolidColorBrush)FindResource("SecondaryColor"),
                    HorizontalAlignment = HorizontalAlignment.Left,
                    Margin = new Thickness(0, 8, 0, 0),
                    Tag = task
                };
                addSubtaskButton.Click += AddSubtask_Click;

                subtasksPanel.Children.Add(addSubtaskButton);
                expander.Content = subtasksPanel;

                footerPanel.Children.Add(expander);
            }
            else
            {
                // Add subtask button for tasks without subtasks
                var addSubtaskButton = new Button
                {
                    Content = "➕ Add Subtask",
                    Style = (Style)FindResource("ModernButton"),
                    Background = (SolidColorBrush)FindResource("SecondaryColor"),
                    HorizontalAlignment = HorizontalAlignment.Left,
                    Margin = new Thickness(0, 8, 0, 0),
                    Tag = task
                };
                addSubtaskButton.Click += AddSubtask_Click;

                footerPanel.Children.Add(addSubtaskButton);
            }

            // Tags
            if (task.Tags != null && task.Tags.Count > 0)
            {
                var tagsPanel = new WrapPanel { Margin = new Thickness(0, 12, 0, 0) };

                foreach (var tag in task.Tags)
                {
                    var tagBorder = new Border
                    {
                        Background = (SolidColorBrush)FindResource("HoverBackground"),
                        BorderBrush = (SolidColorBrush)FindResource("CardBorder"),
                        BorderThickness = new Thickness(1),
                        CornerRadius = new CornerRadius(12),
                        Padding = new Thickness(10, 4, 10, 4),
                        Margin = new Thickness(0, 0, 6, 6)
                    };

                    var tagText = new TextBlock
                    {
                        Text = $"#{tag}",
                        FontSize = 11,
                        Foreground = (SolidColorBrush)FindResource("TextSecondary")
                    };

                    tagBorder.Child = tagText;
                    tagsPanel.Children.Add(tagBorder);
                }

                footerPanel.Children.Add(tagsPanel);
            }

            Grid.SetRow(footerPanel, 2);
            mainGrid.Children.Add(footerPanel);

            card.Child = mainGrid;
            return card;
        }

        private Border CreateSubtaskCard(TaskModel subtask, TaskModel parentTask)
        {
            var card = new Border
            {
                Background = (SolidColorBrush)FindResource("HoverBackground"),
                BorderBrush = (SolidColorBrush)FindResource("CardBorder"),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(6),
                Padding = new Thickness(12),
                Margin = new Thickness(0, 0, 0, 8)
            };

            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            var contentPanel = new StackPanel();

            var titleText = new TextBlock
            {
                Text = subtask.Title,
                FontSize = 14,
                FontWeight = FontWeights.Medium,
                Foreground = (SolidColorBrush)FindResource("TextPrimary"),
                TextWrapping = TextWrapping.Wrap
            };

            var metaText = new TextBlock
            {
                Text = $"{subtask.Status} • {subtask.Priority}",
                FontSize = 11,
                Foreground = (SolidColorBrush)FindResource("TextMuted"),
                Margin = new Thickness(0, 4, 0, 0)
            };

            contentPanel.Children.Add(titleText);
            contentPanel.Children.Add(metaText);

            if (!string.IsNullOrWhiteSpace(subtask.Description))
            {
                var descText = new TextBlock
                {
                    Text = subtask.Description,
                    FontSize = 12,
                    Foreground = (SolidColorBrush)FindResource("TextSecondary"),
                    TextWrapping = TextWrapping.Wrap,
                    Margin = new Thickness(0, 6, 0, 0)
                };
                contentPanel.Children.Add(descText);
            }

            var actionsPanel = new StackPanel { Orientation = Orientation.Horizontal };

            var statusButton = new Button
            {
                Style = (Style)FindResource("IconButton"),
                Content = GetStatusIcon(subtask.Status),
                ToolTip = subtask.Status,
                Tag = new Tuple<TaskModel, TaskModel>(subtask, parentTask)
            };
            statusButton.Click += ToggleSubtaskStatus_Click;

            var editButton = new Button
            {
                Style = (Style)FindResource("IconButton"),
                Content = "✏️",
                ToolTip = "Edit Subtask",
                Tag = new Tuple<TaskModel, TaskModel>(subtask, parentTask)
            };
            editButton.Click += EditSubtask_Click;

            var deleteButton = new Button
            {
                Style = (Style)FindResource("IconButton"),
                Content = "🗑️",
                ToolTip = "Delete Subtask",
                Tag = new Tuple<TaskModel, TaskModel>(subtask, parentTask)
            };
            deleteButton.Click += DeleteSubtask_Click;

            actionsPanel.Children.Add(statusButton);
            actionsPanel.Children.Add(editButton);
            actionsPanel.Children.Add(deleteButton);

            Grid.SetColumn(contentPanel, 0);
            Grid.SetColumn(actionsPanel, 1);
            grid.Children.Add(contentPanel);
            grid.Children.Add(actionsPanel);

            card.Child = grid;
            return card;
        }

        #endregion

        #region Helper Methods

        private string GetStatusIcon(string status)
        {
            return status switch
            {
                "Completed" => "✅",
                "In Progress" => "🔄",
                "On Hold" => "⏸️",
                _ => "⏳"
            };
        }

        private SolidColorBrush GetProjectColorBrush(string color)
        {
            return color switch
            {
                "Green" => new SolidColorBrush((Color)ColorConverter.ConvertFromString(apply.MenuIconColor)),
                "Purple" => new SolidColorBrush((Color)ColorConverter.ConvertFromString(apply.ForegroundSecundary)),
                "Orange" => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFF6D63B")),
                "Red" => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFF63B4F")),
                "Pink" => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFEC4899")),
                _ => new SolidColorBrush((Color)ColorConverter.ConvertFromString(apply.MenuIconColor))
            };
        }

        private SolidColorBrush GetPriorityColorBrush(string priority)
        {
            return priority switch
            {
                "Critical" => (SolidColorBrush)FindResource("DangerColor"),
                "High" => (SolidColorBrush)FindResource("WarningColor"),
                "Medium" => new SolidColorBrush((Color)ColorConverter.ConvertFromString(apply.MenuIconColor)),
                _ => new SolidColorBrush((Color)ColorConverter.ConvertFromString(apply.ForegroundStatus))
            };
        }

        #endregion

        #region Event Handlers - Navigation

        private void ShowAllTasks_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _currentFilter = "all";
                if (ViewTitleText != null)
                    ViewTitleText.Text = "All Tasks";
                if (ViewSubtitleText != null)
                    ViewSubtitleText.Text = "Manage your tasks efficiently";

                // Safely clear ProjectsList selection
                if (ProjectsList != null)
                {
                    ProjectsList.SelectedIndex = -1;
                }

                RefreshTasksView();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error showing all tasks: {ex.Message}", "Error",
                    MessageBox.MessageBoxButton.OK, MessageBox.MessageBoxIcon.Error);
            }
        }

        private void ShowTodayTasks_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _currentFilter = "today";
                if (ViewTitleText != null)
                    ViewTitleText.Text = "Today's Tasks";
                if (ViewSubtitleText != null)
                    ViewSubtitleText.Text = $"Tasks due on {DateTime.Today:dddd, MMMM dd}";

                // Safely clear ProjectsList selection
                if (ProjectsList != null)
                {
                    ProjectsList.SelectedIndex = -1;
                }

                RefreshTasksView();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error showing today's tasks: {ex.Message}", "Error",
                    MessageBox.MessageBoxButton.OK, MessageBox.MessageBoxIcon.Error);
            }
        }

        private void ShowCompletedTasks_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _currentFilter = "completed";
                if (ViewTitleText != null)
                    ViewTitleText.Text = "Completed Tasks";
                if (ViewSubtitleText != null)
                    ViewSubtitleText.Text = "Your achievements";

                // Safely clear ProjectsList selection
                if (ProjectsList != null)
                {
                    ProjectsList.SelectedIndex = -1;
                }

                RefreshTasksView();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error showing completed tasks: {ex.Message}", "Error",
                    MessageBox.MessageBoxButton.OK, MessageBox.MessageBoxIcon.Error);
            }
        }

        private void ShowDailyReport_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _currentFilter = "report";
                if (ViewTitleText != null)
                    ViewTitleText.Text = "Daily Report";
                if (ViewSubtitleText != null)
                    ViewSubtitleText.Text = $"Summary for {DateTime.Today:dddd, MMMM dd, yyyy}";

                // Safely clear ProjectsList selection
                if (ProjectsList != null)
                {
                    ProjectsList.SelectedIndex = -1;
                }

                RefreshTasksView();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error showing daily report: {ex.Message}", "Error",
                   MessageBox.MessageBoxButton.OK, MessageBox.MessageBoxIcon.Error);
            }
        }

        private void ProjectsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ProjectsList.SelectedItem != null)
            {
                _currentFilter = "project";
                RefreshTasksView();
            }
        }
        private void EditProject_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is ProjectModel project)
            {
                // Show edit dialog similar to create
                ProjectNameInput.Text = project.Name;
                ProjectDescriptionInput.Text = project.Description;
                ProjectColorInput.SelectedItem = project.Color;
                _currentProjectId = project.Id;  // Add private int? _currentProjectId; to class fields

                ProjectDialog.Visibility = Visibility.Visible;
            }
        }

        private void DeleteProject_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is ProjectModel project)
            {
                var result = MessageBox.Show($"Are you sure you want to delete '{project.Name}'? This will delete all associated tasks.",
                              "Confirm Delete",
                              MessageBox.MessageBoxButton.YesNo,
                              MessageBox.MessageBoxIcon.Warning);

                if (result == MessageBox.MessageBoxResult.Yes)
                {
                    // Delete associated tasks
                    _tasksCollection.DeleteMany(t => t.ProjectId == project.Id);

                    // Delete project
                    _projectsCollection.Delete(project.Id);

                    // Refresh
                    LoadProjects();
                    if (_currentFilter == "project" && ProjectsList.SelectedItem == project)
                    {
                        _currentFilter = "all";
                    }
                    RefreshTasksView();
                }
            }
        }
        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(SearchBox.Text))
            {
                //searchSVG.Visibility = Visibility.Hidden;
                SearchPlaceHolder.Visibility = Visibility.Hidden;
            }
            else
            {
                //searchSVG.Visibility = Visibility.Visible;
                SearchPlaceHolder.Visibility = Visibility.Visible;
            }
            RefreshTasksView();
        }

        private void RefreshData_Click(object sender, RoutedEventArgs e)
        {
            // Run Animation
            var rotateAnimation = new DoubleAnimation
            {
                From = 0,
                To = 540,
                Duration = TimeSpan.FromSeconds(0.8),
                EasingFunction = new CircleEase { EasingMode = EasingMode.EaseInOut }
            };

            RefreshRotate.BeginAnimation(RotateTransform.AngleProperty, rotateAnimation);
            // Process
            LoadProjects();
            RefreshTasksView();
        }


        #endregion

        #region Event Handlers - Task Dialog

        private void AddTask_Click(object sender, RoutedEventArgs e)
        {
            _currentTaskId = null;
            _currentParentTaskId = null;
            DialogTitle.Text = "Create New Task";
            ClearTaskDialog();
            TaskDialog.Visibility = Visibility.Visible;
        }

        private void AddSubtask_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var parentTask = button?.Tag as TaskModel;

            if (parentTask != null)
            {
                _currentTaskId = null;
                _currentParentTaskId = parentTask.Id;
                DialogTitle.Text = $"Add Subtask to '{parentTask.Title}'";
                ClearTaskDialog();
                TaskProjectInput.SelectedItem = _projects.FirstOrDefault(p => p.Id == parentTask.ProjectId);
                TaskProjectInput.IsEnabled = false;
                TaskDialog.Visibility = Visibility.Visible;
            }
        }

        private void EditTask_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var task = button?.Tag as TaskModel;

            if (task != null)
            {
                _currentTaskId = task.Id;
                _currentParentTaskId = null;
                DialogTitle.Text = "Edit Task";
                PopulateTaskDialog(task);
                TaskDialog.Visibility = Visibility.Visible;
            }
        }

        private void EditSubtask_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var tuple = button?.Tag as Tuple<TaskModel, TaskModel>;

            if (tuple != null)
            {
                var subtask = tuple.Item1;
                var parentTask = tuple.Item2;

                _currentTaskId = subtask.Id;
                _currentParentTaskId = parentTask.Id;
                DialogTitle.Text = "Edit Subtask";
                PopulateTaskDialog(subtask);
                TaskProjectInput.IsEnabled = false;
                TaskDialog.Visibility = Visibility.Visible;
            }
        }

        private void ClearTaskDialog()
        {
            TaskTitleInput.Text = "";
            TaskDescriptionInput.Text = "";
            TaskStatusInput.SelectedIndex = 0;
            TaskPriorityInput.SelectedIndex = 1;
            TaskDueDateInput.SelectedDate = null;
            TaskTagsInput.Text = "";
            TaskProjectInput.IsEnabled = true;

            if (TaskProjectInput.Items.Count > 0)
            {
                TaskProjectInput.SelectedIndex = 0;
            }
        }

        private void PopulateTaskDialog(TaskModel task)
        {
            TaskTitleInput.Text = task.Title;
            TaskDescriptionInput.Text = task.Description ?? "";
            TaskStatusInput.SelectedItem = task.Status;
            TaskPriorityInput.SelectedItem = task.Priority;
            TaskDueDateInput.SelectedDate = task.DueDate;
            TaskTagsInput.Text = task.Tags != null ? string.Join(", ", task.Tags) : "";

            var project = _projects.FirstOrDefault(p => p.Id == task.ProjectId);
            if (project != null)
            {
                TaskProjectInput.SelectedItem = project;
            }
        }

        private void SaveTask_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TaskTitleInput.Text))
            {
                MessageBox.Show("Please enter a task title.", "Validation Error",
                    MessageBox.MessageBoxButton.OK, MessageBox.MessageBoxIcon.Warning);
                return;
            }

            if (TaskProjectInput.SelectedItem == null)
            {
                MessageBox.Show("Please select a project.", "Validation Error",
                    MessageBox.MessageBoxButton.OK, MessageBox.MessageBoxIcon.Warning);
                return;
            }

            var selectedProject = (ProjectModel)TaskProjectInput.SelectedItem;

            var task = _currentTaskId.HasValue
                ? _tasksCollection.FindById(_currentTaskId.Value)
                : new TaskModel();

            task.Title = TaskTitleInput.Text.Trim();
            task.Description = TaskDescriptionInput.Text.Trim();
            task.ProjectId = selectedProject.Id;
            task.Status = TaskStatusInput.SelectedItem?.ToString() ?? "Pending";
            task.Priority = TaskPriorityInput.SelectedItem?.ToString() ?? "Medium";
            task.DueDate = TaskDueDateInput.SelectedDate;
            task.ParentTaskId = _currentParentTaskId;

            var tagsText = TaskTagsInput.Text.Trim();
            if (!string.IsNullOrWhiteSpace(tagsText))
            {
                task.Tags = tagsText.Split(',').Select(t => t.Trim()).Where(t => !string.IsNullOrEmpty(t)).ToList();
            }
            else
            {
                task.Tags = new List<string>();
            }

            if (task.Status == "Completed" && !task.CompletedDate.HasValue)
            {
                task.CompletedDate = DateTime.Now;
            }
            else if (task.Status != "Completed")
            {
                task.CompletedDate = null;
            }

            if (!_currentTaskId.HasValue)
            {
                task.CreatedDate = DateTime.Now;
                task.SubTasks = new List<TaskModel>();
                _tasksCollection.Insert(task);

                // If this is a subtask, update the parent
                if (_currentParentTaskId.HasValue)
                {
                    var parentTask = _tasksCollection.FindById(_currentParentTaskId.Value);
                    if (parentTask != null)
                    {
                        if (parentTask.SubTasks == null)
                            parentTask.SubTasks = new List<TaskModel>();

                        parentTask.SubTasks.Add(task);
                        _tasksCollection.Update(parentTask);
                    }
                }
            }
            else
            {
                _tasksCollection.Update(task);

                // If this is a subtask, update the parent's subtask list
                if (_currentParentTaskId.HasValue)
                {
                    var parentTask = _tasksCollection.FindById(_currentParentTaskId.Value);
                    if (parentTask != null && parentTask.SubTasks != null)
                    {
                        var existingSubtask = parentTask.SubTasks.FirstOrDefault(st => st.Id == task.Id);
                        if (existingSubtask != null)
                        {
                            var index = parentTask.SubTasks.IndexOf(existingSubtask);
                            parentTask.SubTasks[index] = task;
                            _tasksCollection.Update(parentTask);
                        }
                    }
                }
            }

            // Update project progress
            UpdateProjectProgress(selectedProject);

            TaskDialog.Visibility = Visibility.Collapsed;
            LoadProjects();
            RefreshTasksView();
        }

        private void CancelDialog_Click(object sender, RoutedEventArgs e)
        {
            TaskDialog.Visibility = Visibility.Collapsed;
        }

        #endregion

        #region Event Handlers - Task Actions

        private void ToggleTaskStatus_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var task = button?.Tag as TaskModel;

            if (task != null)
            {
                task.Status = task.Status switch
                {
                    "Pending" => "In Progress",
                    "In Progress" => "Completed",
                    "Completed" => "Pending",
                    "On Hold" => "In Progress",
                    _ => "Pending"
                };

                if (task.Status == "Completed")
                {
                    task.CompletedDate = DateTime.Now;
                }
                else
                {
                    task.CompletedDate = null;
                }

                _tasksCollection.Update(task);

                var project = _projectsCollection.FindById(task.ProjectId);
                if (project != null)
                {
                    UpdateProjectProgress(project);
                }

                LoadProjects();
                RefreshTasksView();
            }
        }

        private void ToggleSubtaskStatus_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var tuple = button?.Tag as Tuple<TaskModel, TaskModel>;

            if (tuple != null)
            {
                var subtask = tuple.Item1;
                var parentTask = tuple.Item2;

                subtask.Status = subtask.Status switch
                {
                    "Pending" => "In Progress",
                    "In Progress" => "Completed",
                    "Completed" => "Pending",
                    "On Hold" => "In Progress",
                    _ => "Pending"
                };

                if (subtask.Status == "Completed")
                {
                    subtask.CompletedDate = DateTime.Now;
                }
                else
                {
                    subtask.CompletedDate = null;
                }

                _tasksCollection.Update(subtask);

                // Update parent task's subtask list
                if (parentTask.SubTasks != null)
                {
                    var existingSubtask = parentTask.SubTasks.FirstOrDefault(st => st.Id == subtask.Id);
                    if (existingSubtask != null)
                    {
                        var index = parentTask.SubTasks.IndexOf(existingSubtask);
                        parentTask.SubTasks[index] = subtask;
                        _tasksCollection.Update(parentTask);
                    }
                }

                var project = _projectsCollection.FindById(parentTask.ProjectId);
                if (project != null)
                {
                    UpdateProjectProgress(project);
                }

                LoadProjects();
                RefreshTasksView();
            }
        }

        private void DeleteTask_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var task = button?.Tag as TaskModel;

            if (task != null)
            {
                var result = MessageBox.Show(
                    $"Are you sure you want to delete the task '{task.Title}'?",
                    "Confirm Delete",
                    MessageBox.MessageBoxButton.YesNo, MessageBox.MessageBoxIcon.Question);

                if (result == MessageBox.MessageBoxResult.Yes)
                {
                    // Delete all subtasks
                    if (task.SubTasks != null && task.SubTasks.Count > 0)
                    {
                        foreach (var subtask in task.SubTasks)
                        {
                            _tasksCollection.Delete(subtask.Id);
                        }
                    }

                    _tasksCollection.Delete(task.Id);

                    var project = _projectsCollection.FindById(task.ProjectId);
                    if (project != null)
                    {
                        UpdateProjectProgress(project);
                    }

                    LoadProjects();
                    RefreshTasksView();
                }
            }
        }

        private void DeleteSubtask_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var tuple = button?.Tag as Tuple<TaskModel, TaskModel>;

            if (tuple != null)
            {
                var subtask = tuple.Item1;
                var parentTask = tuple.Item2;

                var result = MessageBox.Show(
                    $"Are you sure you want to delete the subtask '{subtask.Title}'?",
                    "Confirm Delete",
                    MessageBox.MessageBoxButton.YesNo, MessageBox.MessageBoxIcon.Question);

                if (result == MessageBox.MessageBoxResult.Yes)
                {
                    _tasksCollection.Delete(subtask.Id);

                    // Update parent task's subtask list
                    if (parentTask.SubTasks != null)
                    {
                        parentTask.SubTasks.RemoveAll(st => st.Id == subtask.Id);
                        _tasksCollection.Update(parentTask);
                    }

                    var project = _projectsCollection.FindById(parentTask.ProjectId);
                    if (project != null)
                    {
                        UpdateProjectProgress(project);
                    }

                    LoadProjects();
                    RefreshTasksView();
                }
            }
        }

        #endregion

        #region Event Handlers - Project Dialog

        private void AddProject_Click(object sender, RoutedEventArgs e)
        {
            ProjectNameInput.Text = "";
            ProjectDescriptionInput.Text = "";
            ProjectColorInput.SelectedIndex = 0;
            ProjectDialog.Visibility = Visibility.Visible;
        }
        private void ClearProjectInputs()
        {
            ProjectNameInput.Text = string.Empty;
            ProjectDescriptionInput.Text = string.Empty;
            ProjectColorInput.SelectedIndex = 0;
        }
        private void SaveProject_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var name = ProjectNameInput.Text.Trim();
                var description = ProjectDescriptionInput.Text.Trim();
                var color = ProjectColorInput.SelectedItem?.ToString();

                if (string.IsNullOrEmpty(name))
                {
                    MessageBox.Show("Project name is required.", "Error", MessageBox.MessageBoxButton.OK, MessageBox.MessageBoxIcon.Warning);
                    return;
                }

                ProjectModel project;
                if (_currentProjectId.HasValue)
                {
                    // Edit existing
                    project = _projectsCollection.FindById(_currentProjectId.Value);
                    if (project == null) return;

                    project.Name = name;
                    project.Description = description;
                    project.Color = color;

                    _projectsCollection.Update(project);
                }
                else
                {
                    // Create new
                    project = new ProjectModel
                    {
                        Name = name,
                        Description = description,
                        Color = color,
                        CreatedDate = DateTime.Now
                    };
                    _projectsCollection.Insert(project);
                }

                LoadProjects();
                ProjectDialog.Visibility = Visibility.Collapsed;
                ClearProjectInputs();
                _currentProjectId = null;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving project: {ex.Message}", "Error", MessageBox.MessageBoxButton.OK, MessageBox.MessageBoxIcon.Error);
            }
        }

        private void CancelProjectDialog_Click(object sender, RoutedEventArgs e)
        {
            ProjectDialog.Visibility = Visibility.Collapsed;
            ClearProjectInputs();
            _currentProjectId = null;
        }

        #endregion

        #region QR Code Generation

        private void ShowQRCode_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var task = button?.Tag as TaskModel;

            if (task != null)
            {
                try
                {
                    var taskInfo = GenerateTaskInfoString(task);
                    var qrGenerator = new QRCodeGenerator();
                    var qrCodeData = qrGenerator.CreateQrCode(taskInfo, QRCodeGenerator.ECCLevel.Q);
                    var qrCode = new QRCode(qrCodeData);
                    var qrCodeImage = qrCode.GetGraphic(20);

                    using (var memory = new MemoryStream())
                    {
                        qrCodeImage.Save(memory, System.Drawing.Imaging.ImageFormat.Png);
                        memory.Position = 0;

                        var bitmapImage = new BitmapImage();
                        bitmapImage.BeginInit();
                        bitmapImage.StreamSource = memory;
                        bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                        bitmapImage.EndInit();
                        bitmapImage.Freeze();

                        QRCodeImage.Source = bitmapImage;
                    }

                    QRCodeTaskTitle.Text = task.Title;
                    QRCodeDialog.Visibility = Visibility.Visible;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error generating QR code: {ex.Message}", "Error",
                        MessageBox.MessageBoxButton.OK, MessageBox.MessageBoxIcon.Error);
                }
            }
        }

        private string GenerateTaskInfoString(TaskModel task)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"TASK: {task.Title}");

            if (!string.IsNullOrWhiteSpace(task.Description))
                sb.AppendLine($"DESCRIPTION: {task.Description}");

            var project = _projectsCollection.FindById(task.ProjectId);
            if (project != null)
                sb.AppendLine($"PROJECT: {project.Name}");

            sb.AppendLine($"STATUS: {task.Status}");
            sb.AppendLine($"PRIORITY: {task.Priority}");

            if (task.DueDate.HasValue)
                sb.AppendLine($"DUE DATE: {task.DueDate.Value:yyyy-MM-dd}");

            if (task.Tags != null && task.Tags.Count > 0)
                sb.AppendLine($"TAGS: {string.Join(", ", task.Tags)}");

            sb.AppendLine($"CREATED: {task.CreatedDate:yyyy-MM-dd HH:mm}");

            if (task.CompletedDate.HasValue)
                sb.AppendLine($"COMPLETED: {task.CompletedDate.Value:yyyy-MM-dd HH:mm}");

            if (task.SubTasks != null && task.SubTasks.Count > 0)
            {
                sb.AppendLine($"SUBTASKS: {task.SubTasks.Count(st => st.Status == "Completed")}/{task.SubTasks.Count} completed");
            }

            return sb.ToString();
        }

        private void CloseQRDialog_Click(object sender, RoutedEventArgs e)
        {
            QRCodeDialog.Visibility = Visibility.Collapsed;
        }

        #endregion
        private string _currentSort = "Date"; // Add to track current sort mode
        private string _currentStatusFilter = "All"; // Add to track current status filter
        private void ScrollViewer_PreviewMouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
        {
            var scrollViewer = sender as ScrollViewer;
            if (scrollViewer == null)
                return;

            scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset - e.Delta);
            e.Handled = true;
        }

        private void ToggleSidebar_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                double from, to;
                bool collapsing;

                if (SidebarColumn.ActualWidth > 0)
                {
                    from = SidebarColumn.ActualWidth;
                    to = 0;
                    collapsing = true;
                }
                else
                {
                    from = 0;
                    to = 280;
                    collapsing = false;
                }

                var fade = new DoubleAnimation
                {
                    From = collapsing ? 1 : 0,
                    To = collapsing ? 0 : 1,
                    Duration = TimeSpan.FromMilliseconds(200),
                    FillBehavior = FillBehavior.Stop
                };
                SidebarPanel.BeginAnimation(UIElement.OpacityProperty, fade);

                var widthAnimation = new GridLengthAnimation
                {
                    From = new GridLength(from),
                    To = new GridLength(to),
                    Duration = TimeSpan.FromMilliseconds(300),
                    EasingFunction = new CubicEase { EasingMode = EasingMode.EaseInOut }
                };

                SidebarColumn.BeginAnimation(ColumnDefinition.WidthProperty, widthAnimation);

                var timer = new System.Windows.Threading.DispatcherTimer
                {
                    Interval = TimeSpan.FromMilliseconds(310)
                };
                timer.Tick += (s, _) =>
                {
                    timer.Stop();

                    if (collapsing)
                    {
                        SidebarColumn.Width = new GridLength(0);
                        CollapseSidebarButton.Visibility = Visibility.Collapsed;
                        ExpandSidebarButton.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        SidebarColumn.Width = new GridLength(280);
                        CollapseSidebarButton.Visibility = Visibility.Visible;
                        ExpandSidebarButton.Visibility = Visibility.Collapsed;
                    }
                };
                timer.Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error toggling sidebar: {ex.Message}", "Error",
                    MessageBox.MessageBoxButton.OK, MessageBox.MessageBoxIcon.Error);
            }
        }



        private void ShowUpcomingTasks_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _currentFilter = "upcoming";
                ViewTitleText.Text = "Upcoming Tasks";
                ViewSubtitleText.Text = "Tasks due in the future";
                RefreshTasksView();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error showing upcoming tasks: {ex.Message}", "Error",
                    MessageBox.MessageBoxButton.OK, MessageBox.MessageBoxIcon.Error);
            }
        }

        private void SortTasks_Changed(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (SortComboBox.SelectedItem is ComboBoxItem selectedItem)
                {
                    _currentSort = selectedItem.Content.ToString() switch
                    {
                        "Sort by Date" => "Date",
                        "Sort by Priority" => "Priority",
                        "Sort by Status" => "Status",
                        "Sort by Title" => "Title",
                        _ => "Date"
                    };
                    RefreshTasksView();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error sorting tasks: {ex.Message}", "Error",
                    MessageBox.MessageBoxButton.OK, MessageBox.MessageBoxIcon.Error);
            }
        }

        private void FilterTasks_Changed(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (FilterComboBox.SelectedItem is ComboBoxItem selectedItem)
                {
                    _currentStatusFilter = selectedItem.Content.ToString();
                    RefreshTasksView();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error filtering tasks: {ex.Message}", "Error",
                    MessageBox.MessageBoxButton.OK, MessageBox.MessageBoxIcon.Error);
            }
        }

        private async void ExportTasks_Click(object sender, RoutedEventArgs e)
        {
            // Run Animation
            var moveUpAnimation = new DoubleAnimation
            {
                From = 0,
                To = -6,
                Duration = TimeSpan.FromSeconds(0.2),
                AutoReverse = true,
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseInOut }
            };
            ExportTranslate.BeginAnimation(TranslateTransform.YProperty, moveUpAnimation);
            await Task.Delay(150);

            try
            {
                var tasks = _tasksCollection.Find(t => t.ParentTaskId == null).ToList();
                if (tasks.Count == 0)
                {
                    MessageBox.Show("No tasks to export.", "Info",
                        MessageBox.MessageBoxButton.OK, MessageBox.MessageBoxIcon.Error);
                    return;
                }

                var saveFileDialog = new Microsoft.Win32.SaveFileDialog
                {
                    Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*",
                    DefaultExt = ".csv",
                    FileName = $"Tasks_{DateTime.Now:yyyyMMdd}.csv"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    var sb = new StringBuilder();
                    sb.AppendLine("Id,Title,Description,Project,Status,Priority,DueDate,CreatedDate,CompletedDate,Tags");

                    foreach (var task in tasks)
                    {
                        var project = _projectsCollection.FindById(task.ProjectId);
                        var projectName = project?.Name ?? "Unknown";
                        var tags = task.Tags != null ? string.Join(";", task.Tags) : "";
                        var dueDate = task.DueDate.HasValue ? task.DueDate.Value.ToString("yyyy-MM-dd") : "";
                        var completedDate = task.CompletedDate.HasValue ? task.CompletedDate.Value.ToString("yyyy-MM-dd HH:mm") : "";
                        var description = task.Description?.Replace(",", " ").Replace("\r\n", " ").Replace("\n", " ") ?? "";

                        sb.AppendLine($"{task.Id},\"{task.Title}\",\"{description}\",\"{projectName}\",\"{task.Status}\",\"{task.Priority}\",\"{dueDate}\",\"{task.CreatedDate:yyyy-MM-dd HH:mm}\",\"{completedDate}\",\"{tags}\"");
                    }

                    File.WriteAllText(saveFileDialog.FileName, sb.ToString());
                    MessageBox.Show("Tasks exported successfully.", "Success",
                        MessageBox.MessageBoxButton.OK, MessageBox.MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error exporting tasks: {ex.Message}", "Error",
                    MessageBox.MessageBoxButton.OK, MessageBox.MessageBoxIcon.Error);
            }
        }

        // Update the RefreshTasksView method to include sorting and filtering logic
        private void RefreshTasksView()
        {
            // Ensure UI elements are loaded
            if (TasksContainer == null || ViewTitleText == null || ViewSubtitleText == null)
                return;

            TasksContainer.Children.Clear();

            List<TaskModel> tasks = new List<TaskModel>();

            switch (_currentFilter)
            {
                case "all":
                    tasks = _tasksCollection.Find(t => t.ParentTaskId == null).ToList();
                    ViewTitleText.Text = "All Tasks";
                    ViewSubtitleText.Text = "Manage your tasks efficiently";
                    break;

                case "today":
                    var today = DateTime.Today;
                    tasks = _tasksCollection.Find(t => t.ParentTaskId == null &&
                        t.DueDate.HasValue && t.DueDate.Value.Date == today)
                        .ToList();
                    ViewTitleText.Text = "Today's Tasks";
                    ViewSubtitleText.Text = $"Tasks due on {today:dddd, MMMM dd}";
                    break;

                case "upcoming":
                    var tomorrow = DateTime.Today.AddDays(1);
                    tasks = _tasksCollection.Find(t => t.ParentTaskId == null &&
                        t.DueDate.HasValue && t.DueDate.Value.Date >= tomorrow)
                        .ToList();
                    ViewTitleText.Text = "Upcoming Tasks";
                    ViewSubtitleText.Text = "Tasks due in the future";
                    break;

                case "completed":
                    tasks = _tasksCollection.Find(t => t.ParentTaskId == null && t.Status == "Completed")
                        .ToList();
                    ViewTitleText.Text = "Completed Tasks";
                    ViewSubtitleText.Text = "Your achievements";
                    break;

                case "project":
                    if (ProjectsList.SelectedItem is ProjectModel selectedProject)
                    {
                        tasks = _tasksCollection.Find(t => t.ProjectId == selectedProject.Id && t.ParentTaskId == null)
                            .ToList();
                        ViewTitleText.Text = selectedProject.Name;
                        ViewSubtitleText.Text = selectedProject.Description ?? "Project tasks";
                    }
                    break;

                case "report":
                    ShowDailyReportView();
                    return;
            }

            // Apply status filter
            if (_currentStatusFilter != "All Statuses")
            {
                tasks = tasks.Where(t => t.Status == _currentStatusFilter).ToList();
            }

            // Apply search filter
            if (!string.IsNullOrWhiteSpace(SearchBox.Text) && !SearchBox.Text.Contains("🔍"))
            {
                string searchTerm = SearchBox.Text.ToLower();
                tasks = tasks.Where(t =>
                    t.Title.ToLower().Contains(searchTerm) ||
                    (t.Description != null && t.Description.ToLower().Contains(searchTerm)) ||
                    (t.Tags != null && t.Tags.Any(tag => tag.ToLower().Contains(searchTerm)))
                ).ToList();
            }

            // Apply sorting
            tasks = _currentSort switch
            {
                "Date" => tasks.OrderByDescending(t => t.CreatedDate).ToList(),
                "Priority" => tasks.OrderBy(t => t.Priority switch
                {
                    "Critical" => 1,
                    "High" => 2,
                    "Medium" => 3,
                    "Low" => 4,
                    _ => 5
                }).ThenByDescending(t => t.CreatedDate).ToList(),
                "Status" => tasks.OrderBy(t => t.Status).ThenByDescending(t => t.CreatedDate).ToList(),
                "Title" => tasks.OrderBy(t => t.Title).ThenByDescending(t => t.CreatedDate).ToList(),
                _ => tasks.OrderByDescending(t => t.CreatedDate).ToList()
            };

            if (tasks.Count == 0)
            {
                ShowEmptyState();
            }
            else
            {
                foreach (var task in tasks)
                {
                    TasksContainer.Children.Add(CreateTaskCard(task));
                }
            }

            // Update quick stats
            UpdateQuickStats();
        }

        private void UpdateQuickStats()
        {
            try
            {
                var allTasks = _tasksCollection.Find(t => t.ParentTaskId == null).ToList();
                TotalTasksCount.Text = allTasks.Count.ToString();
                ActiveTasksCount.Text = allTasks.Count(t => t.Status != "Completed").ToString();
                CompletedTasksCount.Text = allTasks.Count(t => t.Status == "Completed").ToString();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error updating quick stats: {ex.Message}", "Error",
                    MessageBox.MessageBoxButton.OK, MessageBox.MessageBoxIcon.Error);
            }
        }
        #region Daily Report

        private void ShowDailyReportView()
        {
            TasksContainer.Children.Clear();

            var today = DateTime.Today;
            var allTasks = _tasksCollection.Find(t => t.ParentTaskId == null).ToList();

            // Calculate statistics
            var todayTasks = allTasks.Where(t => t.DueDate.HasValue && t.DueDate.Value.Date == today).ToList();
            var completedToday = allTasks.Where(t => t.CompletedDate.HasValue && t.CompletedDate.Value.Date == today).ToList();
            var overdueTasks = allTasks.Where(t => t.DueDate.HasValue && t.DueDate.Value.Date < today && t.Status != "Completed").ToList();
            var inProgressTasks = allTasks.Where(t => t.Status == "In Progress").ToList();

            var reportCard = new Border
            {
                Style = (Style)FindResource("TaskCard"),
                Padding = new Thickness(24)
            };

            var reportPanel = new StackPanel();

            // Title
            var titleText = new TextBlock
            {
                Text = "📊 Daily Report",
                FontSize = 28,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(apply.ForegroundPrimary)),
                Margin = new Thickness(0, 0, 0, 8)
            };

            // Date
            var dateText = new TextBlock
            {
                Text = today.ToString("dddd, MMMM dd, yyyy"),
                FontSize = 16,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(apply.ForegroundSecundary)),
                Margin = new Thickness(0, 0, 0, 24)
            };

            reportPanel.Children.Add(titleText);
            reportPanel.Children.Add(dateText);

            // Statistics Grid
            var statsGrid = new Grid { Margin = new Thickness(0, 0, 0, 32) };
            statsGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            statsGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            statsGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            statsGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            // Completed Today
            var completedCard = CreateStatCard("✅", "Completed Today", completedToday.Count.ToString(), apply.MenuIconColor);
            Grid.SetColumn(completedCard, 0);
            Grid.SetRow(completedCard, 0);
            statsGrid.Children.Add(completedCard);

            // Due Today
            var dueTodayCard = CreateStatCard("📅", "Due Today", todayTasks.Count.ToString(), apply.MenuIconColor);
            Grid.SetColumn(dueTodayCard, 1);
            Grid.SetRow(dueTodayCard, 0);
            statsGrid.Children.Add(dueTodayCard);

            // In Progress
            var inProgressCard = CreateStatCard("🔄", "In Progress", inProgressTasks.Count.ToString(), apply.MenuIconColor);
            Grid.SetColumn(inProgressCard, 0);
            Grid.SetRow(inProgressCard, 1);
            statsGrid.Children.Add(inProgressCard);

            // Overdue
            var overdueCard = CreateStatCard("⚠️", "Overdue", overdueTasks.Count.ToString(), "#FFF63B4F");
            Grid.SetColumn(overdueCard, 1);
            Grid.SetRow(overdueCard, 1);
            statsGrid.Children.Add(overdueCard);

            reportPanel.Children.Add(statsGrid);

            // Productivity Score
            var totalTasks = allTasks.Count;
            var completedTasks = allTasks.Count(t => t.Status == "Completed");
            var productivityScore = totalTasks > 0 ? (completedTasks * 100.0 / totalTasks) : 0;

            var productivityPanel = new StackPanel { Margin = new Thickness(0, 0, 0, 32) };

            var productivityTitle = new TextBlock
            {
                Text = "Overall Productivity",
                FontSize = 18,
                FontWeight = FontWeights.SemiBold,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(apply.ForegroundPrimary)),
                Margin = new Thickness(0, 0, 0, 12)
            };

            var productivityValue = new TextBlock
            {
                Text = $"{productivityScore:F1}%",
                FontSize = 48,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(apply.MenuIconColor)),
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 0, 0, 8)
            };

            var productivitySubtext = new TextBlock
            {
                Text = $"{completedTasks} of {totalTasks} tasks completed",
                FontSize = 14,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(apply.ForegroundSecundary)),
                HorizontalAlignment = HorizontalAlignment.Center
            };

            productivityPanel.Children.Add(productivityTitle);
            productivityPanel.Children.Add(productivityValue);
            productivityPanel.Children.Add(productivitySubtext);

            reportPanel.Children.Add(productivityPanel);

            // Project Breakdown
            var projectsTitle = new TextBlock
            {
                Text = "Projects Overview",
                FontSize = 18,
                FontWeight = FontWeights.SemiBold,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(apply.ForegroundPrimary)),
                Margin = new Thickness(0, 0, 0, 16)
            };

            reportPanel.Children.Add(projectsTitle);

            foreach (var project in _projects.OrderByDescending(p => p.Progress))
            {
                var projectPanel = new StackPanel { Margin = new Thickness(0, 0, 0, 16) };

                var projectHeader = new Grid();
                projectHeader.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                projectHeader.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

                var projectName = new TextBlock
                {
                    Text = project.Name,
                    FontSize = 16,
                    FontWeight = FontWeights.Medium,
                    Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(apply.ForegroundPrimary))
                };

                var projectProgress = new TextBlock
                {
                    Text = $"{project.Progress:F0}%",
                    FontSize = 16,
                    FontWeight = FontWeights.SemiBold,
                    Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(apply.MenuIconColor))
                };

                Grid.SetColumn(projectName, 0);
                Grid.SetColumn(projectProgress, 1);
                projectHeader.Children.Add(projectName);
                projectHeader.Children.Add(projectProgress);

                var progressBarBorder = new Border
                {
                    Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(apply.BackgroundLightPart)),
                    Height = 8,
                    CornerRadius = new CornerRadius(4),
                    Margin = new Thickness(0, 8, 0, 0)
                };

                var progressBarFill = new Border
                {
                    Background = GetProjectColorBrush(project.Color),
                    Height = 8,
                    CornerRadius = new CornerRadius(4),
                    HorizontalAlignment = HorizontalAlignment.Left
                };

                progressBarBorder.Child = progressBarFill;
                progressBarBorder.SizeChanged += (s, e) =>
                {
                    progressBarFill.Width = e.NewSize.Width * (project.Progress / 100);
                };

                projectPanel.Children.Add(projectHeader);
                projectPanel.Children.Add(progressBarBorder);

                reportPanel.Children.Add(projectPanel);
            }

            // Completed Tasks Today
            if (completedToday.Count > 0)
            {
                var completedTitle = new TextBlock
                {
                    Text = "✅ Completed Today",
                    FontSize = 18,
                    FontWeight = FontWeights.SemiBold,
                    Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(apply.ForegroundPrimary)),
                    Margin = new Thickness(0, 32, 0, 16)
                };

                reportPanel.Children.Add(completedTitle);

                foreach (var task in completedToday.OrderByDescending(t => t.CompletedDate))
                {
                    var taskItem = new TextBlock
                    {
                        Text = $"• {task.Title}",
                        FontSize = 14,
                        Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(apply.ForegroundSecundary)),
                        Margin = new Thickness(16, 0, 0, 8)
                    };
                    reportPanel.Children.Add(taskItem);
                }
            }

            // Overdue Tasks
            if (overdueTasks.Count > 0)
            {
                var overdueTitle = new TextBlock
                {
                    Text = "⚠️ Overdue Tasks",
                    FontSize = 18,
                    FontWeight = FontWeights.SemiBold,
                    Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFF63B4F")),
                    Margin = new Thickness(0, 32, 0, 16)
                };

                reportPanel.Children.Add(overdueTitle);

                foreach (var task in overdueTasks.OrderBy(t => t.DueDate))
                {
                    var taskItem = new TextBlock
                    {
                        Text = $"• {task.Title} (Due: {task.DueDate?.ToString("MMM dd")})",
                        FontSize = 14,
                        Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFF63B4F")),
                        Margin = new Thickness(16, 0, 0, 8)
                    };
                    reportPanel.Children.Add(taskItem);
                }
            }

            reportCard.Child = reportPanel;
            TasksContainer.Children.Add(reportCard);
        }

        private Border CreateStatCard(string icon, string label, string value, string colorHex)
        {
            var card = new Border
            {
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(apply.BackgroundSecundary)),
                BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString(apply.BackgroundLightPart)),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(8),
                Padding = new Thickness(20),
                Margin = new Thickness(0, 0, 12, 12)
            };

            var panel = new StackPanel();

            var iconText = new TextBlock
            {
                Text = icon,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(apply.MenuIconColor)),
                FontSize = 32,
                Margin = new Thickness(0, 0, 0, 8)
            };

            var valueText = new TextBlock
            {
                Text = value,
                FontSize = 36,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(colorHex))
            };

            var labelText = new TextBlock
            {
                Text = label,
                FontSize = 14,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(apply.ForegroundSecundary)),
                Margin = new Thickness(0, 4, 0, 0)
            };

            panel.Children.Add(iconText);
            panel.Children.Add(valueText);
            panel.Children.Add(labelText);

            card.Child = panel;
            return card;
        }

        #endregion

        #region Cleanup

        public void Cleanup()
        {
            //_database?.Dispose();
        }

        #endregion

    }

    #region Data Models

    public class ProjectModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Color { get; set; }
        public DateTime CreatedDate { get; set; }
        public int CompletedTasks { get; set; }
        public int TotalTasks { get; set; }
        public double Progress { get; set; }

        [BsonIgnore]
        public string ProgressText { get; set; }
    }

    public class TaskModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public int ProjectId { get; set; }
        public string Status { get; set; }
        public string Priority { get; set; }
        public DateTime? DueDate { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? CompletedDate { get; set; }
        public List<string> Tags { get; set; }
        public int? ParentTaskId { get; set; }
        public List<TaskModel> SubTasks { get; set; }
    }

    #endregion
}