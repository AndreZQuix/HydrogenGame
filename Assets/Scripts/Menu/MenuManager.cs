using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuManager : MonoBehaviour
{
    public static MenuManager Singleton;

    [SerializeField] Menu[] menus;

    private void Awake()
    {
        Singleton = this;
    }

    public void OpenMenu(string menuName)
    {
        foreach(Menu menu in menus)
        {
            if (string.Equals(menu.name, menuName))
                menu.Open();
            else if (menu.isOpened)
                CloseMenu(menu);
        }
    }

    public void OpenMenu(Menu menu)
    {
        foreach (Menu m in menus)
        {
            if (m.isOpened)
                CloseMenu(m);
        }
        menu.Open();
    }

    public void CloseMenu(Menu menu)
    {
        menu.Close();
    }
}
