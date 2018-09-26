//*********************************************************
//
// Copyright (c) Microsoft. All rights reserved.
// THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
// ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
// IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
// PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.
//
//*********************************************************


using System;
using System.Threading.Tasks;
using Windows.UI.StartScreen;

namespace Utils
{
    sealed class JumpListMenu
    {
        public static async Task Clear()
        {
            var jumpList = await JumpList.LoadCurrentAsync();
            jumpList.Items.Clear();
            await jumpList.SaveAsync();
        }

        public static async Task Add(string argument, string description, string image)
        {
            var jumpList = await JumpList.LoadCurrentAsync();
            var taskItem = JumpListItem.CreateWithArguments(argument, description);
            taskItem.Description = "Exit Alexa";
            taskItem.Logo = new Uri(image);
            jumpList.Items.Add(taskItem);
            await jumpList.SaveAsync();
        }

    }
}

