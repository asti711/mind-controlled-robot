import socketserver
import socket
import threading

import cv2
import numpy as np
import wx
from wx.lib.pubsub import pub

#from .about import description, licence



class MainFrame(wx.Frame):
    """docstring for MainFrame"""
    def __init__(self, parent, title):
        super(MainFrame, self).__init__(parent, title=title)

        #self.threadServer = ThreadServer()
        self.SetIcon(wx.Icon("images/icon.png"))
        self.statusbar = self.CreateStatusBar()
        self.InitUI()

    def InitUI(self):
        menubar = wx.MenuBar()

        fileMenu = wx.Menu()
        fitem = wx.MenuItem(fileMenu, wx.ID_EXIT, 'Quit\tCtrl+Q', 'Quit application')
        fitem.SetBitmap(wx.Bitmap("images/exit.png"))
        fileMenu.AppendItem(fitem)

        helpMenu = wx.Menu()
        aboutitem = wx.MenuItem(helpMenu, wx.ID_ABORT, 'About\tCtrl+A', 'About EmoBotControl')
        aboutitem.SetBitmap(wx.Bitmap("images/about.png"))
        helpMenu.AppendItem(aboutitem)

        menubar.Append(fileMenu, '&File')
        menubar.Append(helpMenu, '&Help')
        self.SetMenuBar(menubar)

        self.Bind(wx.EVT_MENU, self.OnQuit, fitem)
        self.Bind(wx.EVT_MENU, self.OnAboutBox, aboutitem)

        self.statusbar.SetStatusText('Ready')

    def OnQuit(self, e):
        #self.threadServer.server.shutdown()
        
        self.threadServer.server.server_close()
        self.Close()

    def OnAboutBox(self, e):
        #description = description
        #licence = licence

        info = wx.AboutDialogInfo()
        info.SetIcon(wx.Icon("images/icon.png", wx.BITMAP_TYPE_PNG))
        info.SetName('EmoBotControl')
        info.SetVersion('1.0')
        #info.SetDescription(description)
        info.SetCopyright('(C) 2016 Daro Oem')
        info.SetWebSite('http://www.facebook.com/daro.oem')
        #info.SetLicence(licence)
        info.AddDeveloper('Daro Oem')

        wx.AboutBox(info)

if __name__ == '__main__':
    app = wx.App()
    frame = MainFrame(None, title='Mind Bot Control')
    frame.Centre()
    cap = ShowCapture(frame)
    frame.Show()
    app.MainLoop()
