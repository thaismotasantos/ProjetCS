   M S B A M L   `   `   `  ����A  =Projet, Version=1.0.0.0, Culture=neutral, PublicKeyToken=nullclr-namespace:ProjetProjet  5   	   -clr-namespace:Projet.modeleProjet.modele  5   	       Projet.MainWindow   5   	   W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   &localclr-namespace:Projet     5   	   .modeleclr-namespace:Projet.modele     5   	   -   5          � Title$  
MainWindow��5	   	   $	��350��6   $	��525��6)    � 	Resources 5
      ��%    5      .��5      � -   5         �Name$ gridMain��5       � ColumnDefinitions 5   
   �� 5      $��2* �6   �� 5      $��8* �6    � RowDefinitions 5   
   �� 5      $��1* �6   �� 5      $��9* �6   .+�5   
   �� -   5         ��Name$ buttonAddTopic��5   .   $��0��6   $��0��6!   $��Add new topic��6F   6^   �� 5   
   -   5         ��Name$ listBoxChatrooms��5      $��0��6,   $��1��6<   ��6I   �� *	chatrooms+��5      �� 5      .R�5      �� ��6    �� *name+68   5      5      � 5   
   -   5        $ gridChat��5      $��1��6!   $��2��61    5       �� 5!      $��6* �6#   �� 5"      $
��1.5* �6#   �� 5#      $��2* �6#    5%      �� 5&      $
��8.5* �6    �� 5'      $
��1.5* �6    .+�5*      �� -   5         ��Name$ listViewMessages��5*      $��0��61   $��2��6>   ��5+      �� 5,      .R�5-      � .+�5.      �� ��6(   �� *
nickname+6D   �� 5/      $	�� : ��6(   65   �� 50      ��6(   �� *msg+6?   51      52      54      �� 55      -   5         ��Name$ textBoxChat��55   3   $��1��6   $��0��6#   	 �� TextWrapping$
	 Wrap=�6H   $�� ��6\   $��Center=�6d   $��Center=�6�   "
 �� VerticalScrollBarVisibility$

 Auto=�6�   �� 56      -   5        $ buttonSendMessage��56   2   $��1��6   $��1��6"   $
��Send��6M   6\   �� 58      -	   5        $ listBoxParticipants��58      $��2��63   $��2��6C   ��59      �� 5:      .R�5;      �� ��6$   �� 67   5<      5>      5?      5B      5C      