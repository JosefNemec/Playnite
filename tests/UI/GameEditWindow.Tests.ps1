Describe "Edit Window - Basic Test" {
    BeforeAll {
        $windowMain = & (Join-Path $PSScriptRoot "..\Mapping\MainWindow.ps1")
        $windowGameEdit = & (Join-Path $PSScriptRoot "..\Mapping\GameEditWindow.ps1")
        $windowOpenFile = & (Join-Path $PSScriptRoot "..\Mapping\OpenFileWindow.ps1")     

        $editData = @{
            "Name" = "AA New Name";
            "Genres" = "Genre1, Genre2";
            "ReleaseDate" = [datetime]::Now.ToShortDateString();
            "Developers" = "Developer1";
            "Publishers" = "Publisher";
            "Categories" = "Cat1, Cat2, Cat3";
            "StoreURL" = "New store url";
            "WikiURL" = "New wiki url";
            "ForumsURL" = "New forum url";
            "Description" = "New description";
            "Icon" = Join-Path $PSScriptRoot "..\TestFiles\TestIcon.png";
            "Image" = Join-Path $PSScriptRoot "..\TestFiles\TestCover.jpg";
        }
    }

    It "Edit window can be opened" {
        $windowMain.ListBoxGames.GetItems()[(Get-Random -Minimum 0 -Maximum 5)].ClickRight()
        $windowMain.PopupMenu.InvokeItem("Edit...")
        $windowGameEdit.WaitForObject(5000)
        $windowGameEdit.Close()
    }

    It "Cancel button doesn't save changes" {
        $itemIndex = Get-Random -Minimum 0 -Maximum 5
        $windowMain.ListBoxGames.GetItems()[$itemIndex].ClickRight()
        $windowMain.PopupMenu.InvokeItem("Edit...")
        $windowGameEdit.WaitForObject(5000)

        # basic fileds
        $windowGameEdit.TextName.SetValue($editData.Name)
        $windowGameEdit.TextGenres.SetValue($editData.Genres)
        $windowGameEdit.TextReleaseDate.SetValue($editData.ReleaseDate)
        $windowGameEdit.TextDeveloper.SetValue($editData.Developers)
        $windowGameEdit.TextPublisher.SetValue($editData.Publishers)
        $windowGameEdit.TextCategories.SetValue($editData.Categories)
        $windowGameEdit.TextStore.SetValue($editData.StoreURL)
        $windowGameEdit.TextWiki.SetValue($editData.WikiURL)
        $windowGameEdit.TextForums.SetValue($editData.ForumsURL)
        $windowGameEdit.TextDescription.SetValue($editData.Description)
        # images
        $windowGameEdit.ButtonSelectIcon.Invoke()
        $windowOpenFile.EditFileName.SetValue($editData.Icon)
        $windowOpenFile.ButtonOpen.Invoke()
        $windowGameEdit.ButtonSelectImage.Invoke()
        $windowOpenFile.EditFileName.SetValue($editData.Image)
        $windowOpenFile.ButtonOpen.Invoke()
        # tasks
        $windowGameEdit.TabControlMain.SelectItem("Actions")
        $windowGameEdit.TaskPlay.TextName.SetValue("TestName")
        $windowGameEdit.TaskPlay.TextPath.SetValue("TestPath")

        $windowGameEdit.ButtonCancel.Invoke()        
        $windowMain.ListBoxGames.GetItems()[$itemIndex].ClickRight()
        $windowMain.PopupMenu.InvokeItem("Edit...")

        $windowGameEdit.TextName.GetValue() | Should Not Be $editData.Name
        $windowGameEdit.TextGenres.GetValue() | Should Not Be $editData.Genres
        $windowGameEdit.TextReleaseDate.GetValue() | Should Not Be $editData.ReleaseDate
        $windowGameEdit.TextDeveloper.GetValue() | Should Not Be $editData.Developers
        $windowGameEdit.TextPublisher.GetValue() | Should Not Be $editData.Publishers
        $windowGameEdit.TextCategories.GetValue() | Should Not Be $editData.Categories
        $windowGameEdit.TextStore.GetValue() | Should Not Be $editData.StoreURL
        $windowGameEdit.TextWiki.GetValue() | Should Not Be $editData.WikiURL
        $windowGameEdit.TextForums.GetValue() | Should Not Be $editData.ForumsURL
        $windowGameEdit.TextDescription.GetValue() | Should Not Be $editData.Description
        $windowGameEdit.ImageIcon.GetObject().Current.HelpText | Should Not BeLike "images/custom*"
        $windowGameEdit.ImageImage.GetObject().Current.HelpText | Should Not BeLike "images/custom*"
        $windowGameEdit.TabControlMain.SelectItem("Actions")
        $windowGameEdit.TaskPlay.TextName.GetValue() | Should Not Be "TestName"
        $windowGameEdit.TaskPlay.TextPath.GetValue() | Should Not Be "TestPath"

        $windowGameEdit.Close()
    }

    It "OK button saves changes" {
        $itemIndex = Get-Random -Minimum 0 -Maximum 5
        $windowMain.ListBoxGames.GetItems()[$itemIndex].Select()
        $windowMain.ButtonMore.Invoke()
        $windowMain.PopupMenu.InvokeItem("Edit...")
        $windowGameEdit.WaitForObject(5000)

        # basic fields
        $windowGameEdit.TextName.SetValue($editData.Name)
        $windowGameEdit.TextGenres.SetValue($editData.Genres)
        $windowGameEdit.TextReleaseDate.SetValue($editData.ReleaseDate)
        $windowGameEdit.TextDeveloper.SetValue($editData.Developers)
        $windowGameEdit.TextPublisher.SetValue($editData.Publishers)
        $windowGameEdit.TextCategories.SetValue($editData.Categories)
        $windowGameEdit.TextStore.SetValue($editData.StoreURL)
        $windowGameEdit.TextWiki.SetValue($editData.WikiURL)
        $windowGameEdit.TextForums.SetValue($editData.ForumsURL)
        $windowGameEdit.TextDescription.SetValue($editData.Description)
        # images
        $windowGameEdit.ButtonSelectIcon.Invoke()
        $windowOpenFile.EditFileName.SetValue($editData.Icon)
        $windowOpenFile.ButtonOpen.Invoke()
        $windowGameEdit.ButtonSelectImage.Invoke()
        $windowOpenFile.EditFileName.SetValue($editData.Image)
        $windowOpenFile.ButtonOpen.Invoke()
        # tasks
        $windowGameEdit.TabControlMain.SelectItem("Actions")
        $windowGameEdit.TaskPlay.TextName.SetValue("TestName")
        $windowGameEdit.TaskPlay.TextPath.SetValue("TestPath")

        $windowGameEdit.ButtonOK.Invoke()        
        $windowMain.ListBoxGames.GetItems()[0].Select()
        $windowMain.ButtonMore.Invoke()
        $windowMain.PopupMenu.InvokeItem("Edit...")

        $windowGameEdit.TextName.GetValue() | Should Be $editData.Name
        $windowGameEdit.TextGenres.GetValue() | Should Be $editData.Genres
        $windowGameEdit.TextReleaseDate.GetValue() | Should Be $editData.ReleaseDate
        $windowGameEdit.TextDeveloper.GetValue() | Should Be $editData.Developers
        $windowGameEdit.TextPublisher.GetValue() | Should Be $editData.Publishers
        $windowGameEdit.TextCategories.GetValue() | Should Be $editData.Categories
        $windowGameEdit.TextStore.GetValue() | Should Be $editData.StoreURL
        $windowGameEdit.TextWiki.GetValue() | Should Be $editData.WikiURL
        $windowGameEdit.TextForums.GetValue() | Should Be $editData.ForumsURL
        $windowGameEdit.TextDescription.GetValue() | Should Be $editData.Description
        $windowGameEdit.ImageIcon.GetObject().Current.HelpText | Should BeLike "images/custom*"
        $windowGameEdit.ImageImage.GetObject().Current.HelpText | Should BeLike "images/custom*"
        $windowGameEdit.TabControlMain.SelectItem("Actions")
        $windowGameEdit.TaskPlay.TextName.GetValue() | Should Be "TestName"
        $windowGameEdit.TaskPlay.TextPath.GetValue() | Should Be "TestPath"

        $windowGameEdit.Close()
    }
}

Describe "Edit Window - Bulk editing" {

}

Describe "Edit Window - Categories" {
    BeforeAll {
        $windowMain = & (Join-Path $PSScriptRoot "..\Mapping\MainWindow.ps1")
        $windowGameEdit = & (Join-Path $PSScriptRoot "..\Mapping\GameEditWindow.ps1")
        $windowCategoryConfig = & (Join-Path $PSScriptRoot "..\Mapping\CategoryConfigWindow.ps1")        
    }

    It "Cancel doesn't change category" { 
        $itemIndex = Get-Random -Minimum 0 -Maximum 5
        $windowMain.ListBoxGames.GetItems()[$itemIndex].ClickRight()
        $windowMain.PopupMenu.InvokeItem("Edit...")
        $windowGameEdit.WaitForObject(5000)
        
        $currentCat = $windowGameEdit.TextCategories.GetValue()
        $windowGameEdit.ButtonPickCat.Invoke()
        $windowCategoryConfig.WaitForObject(2000)
        $windowCategoryConfig.TextNewCat.SetValue("NewCat1")
        $windowCategoryConfig.ButtonAddCat.Invoke()
        $windowCategoryConfig.ButtonCancel.Invoke()
        $windowGameEdit.TextCategories.GetValue() | Should Be $currentCat

        $windowGameEdit.Close()
    }

    It "OK changes category" {
        $itemIndex = Get-Random -Minimum 0 -Maximum 5
        $windowMain.ListBoxGames.GetItems()[$itemIndex].ClickRight()
        $windowMain.PopupMenu.InvokeItem("Edit...")
        $windowGameEdit.WaitForObject(5000)
        
        $windowGameEdit.ButtonPickCat.Invoke()
        $windowCategoryConfig.WaitForObject(2000)
        $windowCategoryConfig.TextNewCat.SetValue("NewCat1")
        $windowCategoryConfig.ButtonAddCat.Invoke()
        $windowCategoryConfig.ButtonOK.Invoke()
        $windowGameEdit.TextCategories.GetValue() | Should Be "NewCat1"

        $windowGameEdit.Close()
    }
}

Describe "Edit Window - Icon from EXE" {

}

Describe "Edit Window - Metadata Download" {

}

Describe "Edit Window - Tasks" {

}