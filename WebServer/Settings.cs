/*****************************************************************************************
 *  p2p-player
 *  An audio player developed in C# based on a shared base to obtain the music from.
 * 
 *  Copyright (C) 2010-2011 Dario Mazza, Sebastiano Merlino
 *
 *  This program is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU Affero General Public License as
 *  published by the Free Software Foundation, either version 3 of the
 *  License, or (at your option) any later version.
 *
 *  This program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU Affero General Public License for more details.
 *
 *  You should have received a copy of the GNU Affero General Public License
 *  along with this program.  If not, see <http://www.gnu.org/licenses/>.
 *  
 *  Dario Mazza (dariomzz@gmail.com)
 *  Sebastiano Merlino (etr@pensieroartificiale.com)
 *  Full Source and Documentation available on Google Code Project "p2p-player", 
 *  see <http://code.google.com/p/p2p-player/>
 *
 ******************************************************************************************/

﻿namespace PeerPlayer.Properties {
    
    
    // La classe consente la gestione di eventi specifici sulla classe delle impostazioni:
    //  L'evento SettingChanging viene generato prima della modifica di un valore dell'impostazione.
    //  L'evento PropertyChanged viene generato dopo la modifica di un valore dell'impostazione.
    //  L'evento SettingsLoaded viene generato dopo il caricamento dei valori dell'impostazione.
    //  L'evento SettingsSaving viene generato prima del salvataggio dei valori dell'impostazione.
    internal sealed partial class Settings {
        
        public Settings() {
            // // Per aggiungere gestori eventi per il salvataggio e la modifica delle impostazioni, rimuovere i commenti dalle righe seguenti:
            //
            // this.SettingChanging += this.SettingChangingEventHandler;
            //
            // this.SettingsSaving += this.SettingsSavingEventHandler;
            //
        }
        
        private void SettingChangingEventHandler(object sender, System.Configuration.SettingChangingEventArgs e) {
            // Aggiungere qui il codice per gestire l'evento SettingChangingEvent.
        }
        
        private void SettingsSavingEventHandler(object sender, System.ComponentModel.CancelEventArgs e) {
            // Aggiungere qui il codice per gestire l'evento SettingsSaving.
        }
    }
}
