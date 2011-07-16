#!/bin/sh

#sed -i '1s/\(.*\)/\/* kasdkaosdkoaskdoaskdoaskdoaskokaos \n asdkoakdoakdosakdoaskdosakdoad $ *\/\n\1/' prova.txt

AGPL="1s/\(.*\)/<!--\n\/*****************************************************************************************\n *  p2p-player\n *  An audio player developed in C# based on a shared base to obtain the music from.\n * \n *  Copyright \(C\) 2010-2011 Dario Mazza, Sebastiano Merlino\n *\n *  This program is free software: you can redistribute it and\/or modify\n *  it under the terms of the GNU Affero General Public License as\n *  published by the Free Software Foundation, either version 3 of the\n *  License, or \(at your option\) any later version.\n *\n *  This program is distributed in the hope that it will be useful,\n *  but WITHOUT ANY WARRANTY; without even the implied warranty of\n *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the\n *  GNU Affero General Public License for more details.\n *\n *  You should have received a copy of the GNU Affero General Public License\n *  along with this program.  If not, see <http:\/\/www.gnu.org\/licenses\/>.\n *  \n *  Dario Mazza \(dariomzz@gmail.com\)\n *  Sebastiano Merlino \(etr@pensieroartificiale.com\)\n *  Full Source and Documentation available on Google Code Project \"p2p-player\", \n *  see <http:\/\/code.google.com\/p\/p2p-player\/>\n *\n ******************************************************************************************\/\n-->\n\1/"

echo "sed -i ${AGPL} ..."
for i in `find -iname "*.xaml"`; do sed -i "${AGPL}" $i ; done;

#sed -i '1s/\(.*\)/$AGPL\1/' Metrics/FileQualityCoefficient.cs

