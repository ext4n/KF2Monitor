using System.Collections.Generic;

namespace KF2Monitor.ViewModels
{
    public class FaqItem
    {
        public string Question { get; set; } = "";
        public string Answer { get; set; } = "";
    }

    public static class FaqManager
    {
        public static List<FaqItem> GetFaqs(string langCode)
        {
            return langCode switch
            {
                "et" => new List<FaqItem>
                {
                    new FaqItem { Question = "Kuidas vidinat lisada?", Answer = "Minge avakuvale, vajutage pikalt tühjale kohale, valige \"Vidinad\" ja leidke KF2Monitor." },
                    new FaqItem { Question = "Miks vidin ei uuene?", Answer = "Veenduge, et rakenduse akusäästja on välja lülitatud ja internetiühendus on stabiilne." },
                    new FaqItem { Question = "Kuidas mängijaid jälgida?", Answer = "Luba jälgimine seadetes ja sisesta mängijate täpsed hüüdnimed komadega eraldatult. Saate teateid, kui nad serveriga liituvad või sealt lahkuvad." },
                    new FaqItem { Question = "Kuidas töötavad serveri täitumise teavitused?", Answer = "Määrake seadetes mängijate piirang. Teid teavitatakse, kui server saavutab selle arvu. Märkus: see töötab ainult \"Serverite nähtavus\" all sisse lülitatud serverite puhul." },
                    new FaqItem { Question = "Kuidas rakendust uuendada?", Answer = "Rakendus kontrollib uuendusi automaatselt kord päevas. Kui uus versioon on saadaval, ilmub uuenduste vahekaardile märge \"UUS\"." },
                    new FaqItem { Question = "Kuidas vidina tausta kohandada?", Answer = "Vidina vahekaardil saate valida pildi, muuta selle asukohta ning kasutada valikuid \"Täissuuruses taustapilt\" või \"Hajuta pilt tausta\"." },
                    new FaqItem { Question = "Kas ma saan vidina paigutust muuta?", Answer = "Jah! Vajutage rakenduse eelvaates vidinale pikalt, et avada redaktor. Saate vahetada päise elemente, vasak/parem pooli ning laine/mängijate teksti." },
                    new FaqItem { Question = "Milleks on nupp \"Värskenda pilvest\"?", Answer = "See laadib alla uusima serverite IP/pordi loendi, kui need on muutunud. Kasutage seda, kui serverid on märgitud kui 'Offline'." },
                    new FaqItem { Question = "Kas ma saan jälgida oma servereid?", Answer = "Vaikimisi on vidin seotud KF2MOD-EU serveritega. Kogenud kasutajad saavad rakenduse andmetes faili 'servers.json' käsitsi üle kirjutada." },
                    new FaqItem { Question = "Miks ma näen \"Timeout\" või \"Offline\"?", Answer = "Server võib taaskäivituda, teie pakkuja blokeerib UDP-liiklust või akusäästja piirab taustatööd." },
                    new FaqItem { Question = "Kas vidin kurnab mu akut?", Answer = "Väga vähe. See ärkab ainult määratud intervalli järel, laadib paar kilobaiti ja läheb tagasi puhkerežiimi." },
                    new FaqItem { Question = "Kuidas tühjendada jälgitavate mängijate loendit?", Answer = "Kustutage lihtsalt hüüdnimed seadete tekstikastist ja vajutage 'Salvesta konfiguratsioon'." },
                    new FaqItem { Question = "Kas ma saan kasutada läbipaistvat tausta?", Answer = "Jah! Värvivalijas seadke 'Alpha' liugur 0-le ja veenduge, et taustapilti pole valitud." },
                    new FaqItem { Question = "Miks märguanded hilinevad?", Answer = "Android piirab aku säästmiseks täpseid taustategevusi. Vidin värskendab nii sageli, kui süsteem lubab." },
                    new FaqItem { Question = "Kuhu teatada veast või pakkuda uut funktsiooni?", Answer = "Kasutage vahekaardil Teave asuvaid nuppe 'GitHub' või 'Discord', et võtta ühendust arendajaga." }
                },
                "sk" => new List<FaqItem>
                {
                    new FaqItem { Question = "Ako pridať miniaplikáciu?", Answer = "Prejdite na domovskú obrazovku, dlho stlačte prázdne miesto, vyberte \"Miniaplikácie\" a nájdite KF2Monitor." },
                    new FaqItem { Question = "Prečo sa miniaplikácia neaktualizuje?", Answer = "Uistite sa, že optimalizácia batérie je pre aplikáciu vypnutá a internetové pripojenie je stabilné." },
                    new FaqItem { Question = "Ako sledovať hráčov?", Answer = "Povoľte sledovanie v nastaveniach a zadajte presné prezývky hráčov oddelené čiarkami. Dostanete upozornenie, keď sa pripoja alebo odpoja." },
                    new FaqItem { Question = "Ako fungujú upozornenia na preplnený server?", Answer = "Nastavte hranicu hráčov v nastaveniach. Budete upozornení, keď server dosiahne tento počet. Poznámka: Funguje to len pre servery povolené v sekcii \"Viditeľnosť serverov\"." },
                    new FaqItem { Question = "Ako aktualizovať aplikáciu?", Answer = "Aplikácia kontroluje aktualizácie automaticky raz denne. Keď je dostupná nová verzia, na karte Aktualizácie sa zobrazí odznak \"NOVÉ\"." },
                    new FaqItem { Question = "Ako prispôsobiť pozadie miniaplikácie?", Answer = "Na karte Widget môžete vybrať obrázok, upraviť jeho polohu a použiť možnosti \"Obrázok v plnej veľkosti\" alebo \"Postupné splývanie\" pre lepší vzhľad." },
                    new FaqItem { Question = "Môžem zmeniť rozloženie miniaplikácie?", Answer = "Áno! Dlhým stlačením miniaplikácie v náhľade otvoríte Editor. Môžete vymeniť prvky hlavičky, ľavú/pravú stranu a text vlny/hráčov." },
                    new FaqItem { Question = "Na čo slúži tlačidlo \"Aktualizovať z cloudu\"?", Answer = "Stiahne najnovší zoznam IP/Portov serverov, ak sa zmenili. Použite to, ak sú servery zobrazené ako 'Offline'." },
                    new FaqItem { Question = "Môžem sledovať vlastné servery?", Answer = "V predvolenom nastavení je widget viazaný na KF2MOD-EU. Pokročilí používatelia môžu manuálne prepísať 'servers.json' v dátach aplikácie." },
                    new FaqItem { Question = "Prečo vidím \"Timeout\" alebo \"Offline\"?", Answer = "Server sa možno reštartuje, váš poskytovateľ blokuje UDP alebo optimalizácia batérie obmedzuje prácu na pozadí." },
                    new FaqItem { Question = "Vybíja miniaplikácia moju batériu?", Answer = "Veľmi málo. Zobudí sa len v nastavenom intervale, stiahne pár kilobajtov a vráti sa do režimu spánku." },
                    new FaqItem { Question = "Ako vymazať zoznam sledovaných hráčov?", Answer = "Stačí vymazať prezývky z textového poľa v nastaveniach a stlačiť 'Uložiť konfiguráciu'." },
                    new FaqItem { Question = "Môžem použiť priehľadné pozadie?", Answer = "Áno! V palete farieb nastavte posuvník 'Alpha' na 0 a uistite sa, že nie je vybraný žiadny obrázok." },
                    new FaqItem { Question = "Prečo meškajú upozornenia?", Answer = "Android obmedzuje presné úlohy na pozadí kvôli šetreniu batérie. Widget sa aktualizuje tak často, ako to systém povolí." },
                    new FaqItem { Question = "Kde môžem nahlásiť chybu alebo navrhnúť funkciu?", Answer = "Použite tlačidlá 'GitHub' alebo 'Discord' v karte Info a kontaktujte vývojára alebo komunitu." }
                },
                "lt" => new List<FaqItem>
                {
                    new FaqItem { Question = "Kaip pridėti valdiklį?", Answer = "Eikite į pagrindinį ekraną, ilgai paspauskite tuščią vietą, pasirinkite \"Valdikliai\" ir raskite KF2Monitor." },
                    new FaqItem { Question = "Kodėl valdiklis neatsinaujina?", Answer = "Įsitikinkite, kad programai išjungtas baterijos optimizavimas ir interneto ryšys yra stabilus." },
                    new FaqItem { Question = "Kaip stebėti žaidėjus?", Answer = "Įjunkite stebėjimą nustatymuose ir įveskite tikslius žaidėjų slapyvardžius atskirtus kableliais. Gausite pranešimus, kai jie prisijungs arba atsijungs." },
                    new FaqItem { Question = "Kaip veikia serverio užpildymo pranešimai?", Answer = "Nustatykite žaidėjų ribą nustatymuose. Būsite informuoti, kai serveryje bus pasiektas šis skaičius. Pastaba: tai veikia tik tiems serveriams, kurie įjungti skiltyje \"Serverių matomumas\"." },
                    new FaqItem { Question = "Kaip atnaujinti programą?", Answer = "Programa automatiškai tikrina atnaujinimus kartą per dieną. Kai bus nauja versija, Atnaujinimų skirtuke pasirodys ženkliukas \"NAUJA\"." },
                    new FaqItem { Question = "Kaip pritaikyti valdiklio foną?", Answer = "Valdiklio skirtuke galite pasirinkti paveikslėlį, koreguoti jo padėtį ir naudoti parinktis \"Viso dydžio fono paveikslėlis\" arba \"Išblukinti paveikslėlį\", kad geriau derėtų." },
                    new FaqItem { Question = "Ar galiu pakeisti valdiklio išdėstymą?", Answer = "Taip! Ilgai paspauskite valdiklį peržiūroje, kad atidarytumėte redaktorių. Galite sukeisti antraštės elementus, kairę/dešinę puses ir bangos/žaidėjų tekstą." },
                    new FaqItem { Question = "Kam skirtas mygtukas \"Atnaujinti iš debesies\"?", Answer = "Jis atsisiunčia naujausią serverių IP/prievadų sąrašą, jei jie pasikeitė. Naudokite tai, jei serveriai rodomi kaip 'Offline'." },
                    new FaqItem { Question = "Ar galiu stebėti savo serverius?", Answer = "Pagal numatytuosius nustatymus valdiklis yra susietas su KF2MOD-EU. Pažengę vartotojai gali rankiniu būdu perrašyti „servers.json“ programos duomenyse." },
                    new FaqItem { Question = "Kodėl matau \"Timeout\" arba \"Offline\"?", Answer = "Serveris gali būti paleidžiamas iš naujo, tiekėjas blokuoja UDP arba baterijos optimizavimas riboja foninį darbą." },
                    new FaqItem { Question = "Ar valdiklis eikvoja bateriją?", Answer = "Labai mažai. Jis atsibunda nustatytu intervalu, atsiunčia kelis kilobaitus ir vėl užmiega." },
                    new FaqItem { Question = "Kaip išvalyti stebimų žaidėjų sąrašą?", Answer = "Tiesiog ištrinkite pravardes iš teksto laukelio nustatymuose ir paspauskite „Išsaugoti konfigūraciją“." },
                    new FaqItem { Question = "Ar galiu naudoti skaidrų foną?", Answer = "Taip! Spalvų parinkiklyje nustatykite „Alpha“ slankiklį ties 0 ir įsitikinkite, kad nepasirinktas joks fono paveikslėlis." },
                    new FaqItem { Question = "Kodėl pranešimai vėluoja?", Answer = "„Android“ riboja tikslias fonines užduotis, kad taupytų bateriją. Valdiklis atsinaujina taip dažnai, kaip leidžia sistema." },
                    new FaqItem { Question = "Kur galiu pranešti apie klaidą ar pasiūlyti funkciją?", Answer = "Naudokite „GitHub“ arba „Discord“ mygtukus skirtuke „Apie“, kad susisiektumėte su kūrėju." }
                },
                "lv" => new List<FaqItem>
                {
                    new FaqItem { Question = "Kā pievienot logrīku?", Answer = "Dodieties uz sākuma ekrānu, turiet nospiestu tukšu vietu, izvēlieties \"Logrīki\" un atrodiet KF2Monitor." },
                    new FaqItem { Question = "Kāpēc logrīks netiek atjaunināts?", Answer = "Pārliecinieties, vai lietotnei ir atspējota akumulatora optimizācija un interneta savienojums ir stabils." },
                    new FaqItem { Question = "Kā izsekot spēlētājus?", Answer = "Iespējojiet izsekošanu iestatījumos un ievadiet precīzus spēlētāju segvārdus, atdalītus ar komatiem. Jūs saņemsiet paziņojumus, kad viņi pievienojas vai pamet serveri." },
                    new FaqItem { Question = "Kā darbojas servera piepildīšanās paziņojumi?", Answer = "Iestatiet spēlētāju slieksni. Jūs saņemsiet paziņojumu, kad serveris sasniegs šo skaitu. Piezīme: Tas darbojas tikai tiem serveriem, kas ir iespējoti sadaļā \"Serveru redzamība\"." },
                    new FaqItem { Question = "Kā atjaunināt lietotni?", Answer = "Lietotne automātiski pārbauda atjauninājumus reizi dienā. Kad būs pieejama jauna versija, Atjauninājumu cilnē parādīsies nozīmīte \"JAUNS\"." },
                    new FaqItem { Question = "Kā pielāgot logrīka fonu?", Answer = "Logrīka cilnē varat izvēlēties attēlu, pielāgot tā pozīciju un izmantot \"Pilna izmēra attēls\" vai \"Sapludināt attēlu ar fonu\" iespējas labākam izskatam." },
                    new FaqItem { Question = "Vai es varu mainīt logrīka izkārtojumu?", Answer = "Jā! Turiet nospiestu logrīku priekšskatījumā, lai atvērtu redaktoru. Jūs varat apmainīt galvenes elementus, kreiso/labo pusi un viļņa/spēlētāju tekstu." },
                    new FaqItem { Question = "Kam paredzēta poga \"Atjaunināt no mākoņa\"?", Answer = "Tā lejupielādē jaunāko serveru IP/porta sarakstu, ja tie ir mainījušies. Izmantojiet to, ja serveri rāda 'Offline'." },
                    new FaqItem { Question = "Vai varu izsekot savus serverus?", Answer = "Pēc noklusējuma logrīks ir saistīts ar KF2MOD-EU. Pieredzējuši lietotāji lietotnes datos var manuāli pārrakstīt \"servers.json\"." },
                    new FaqItem { Question = "Kāpēc es redzu \"Timeout\" vai \"Offline\"?", Answer = "Serveris var tikt restartēts, pakalpojumu sniedzējs bloķē UDP, vai akumulatora optimizācija ierobežo fona darbību." },
                    new FaqItem { Question = "Vai logrīks izlādē manu akumulatoru?", Answer = "Ļoti maz. Tas pamostas iestatītajā intervālā, lejupielādē dažus kilobaitus un atkal aizmieg." },
                    new FaqItem { Question = "Kā notīrīt izsekoto spēlētāju sarakstu?", Answer = "Vienkārši izdzēsiet segvārdus no tekstlodziņa iestatījumos un nospiediet 'Saglabāt konfigurāciju'." },
                    new FaqItem { Question = "Vai es varu izmantot caurspīdīgu fonu?", Answer = "Jā! Krāsu atlasītājā iestatiet slīdni 'Alpha' uz 0 un pārliecinieties, ka nav atlasīts fona attēls." },
                    new FaqItem { Question = "Kāpēc paziņojumi kavējas?", Answer = "Android ierobežo precīzus fona uzdevumus akumulatora taupīšanai. Logrīks tiek atjaunināts tik bieži, cik sistēma atļauj." },
                    new FaqItem { Question = "Kur es varu ziņot par kļūdu vai ieteikt funkciju?", Answer = "Izmantojiet pogas \"GitHub\" vai \"Discord\" cilnē Par, lai sazinātos ar izstrādātāju." }
                },
                "fr" => new List<FaqItem>
                {
                    new FaqItem { Question = "Comment ajouter le widget?", Answer = "Allez sur l'écran d'accueil, appuyez longuement sur un espace vide, sélectionnez \"Widgets\" et trouvez KF2Monitor." },
                    new FaqItem { Question = "Pourquoi le widget ne se met-il pas à jour?", Answer = "Assurez-vous que l'optimisation de la batterie est désactivée pour l'application et que votre connexion Internet est stable." },
                    new FaqItem { Question = "Comment suivre les joueurs?", Answer = "Activez le suivi dans les paramètres et entrez les pseudos exacts des joueurs séparés par des virgules. Vous recevrez des notifications lorsqu'ils rejoignent ou quittent un serveur." },
                    new FaqItem { Question = "Comment fonctionnent les notifications de remplissage du serveur?", Answer = "Définissez un seuil de joueurs. Vous serez averti lorsqu'un serveur atteindra ce nombre. Remarque: cela ne fonctionne que pour les serveurs activés dans \"Visibilité des serveurs\"." },
                    new FaqItem { Question = "Comment mettre à jour l'application?", Answer = "L'application recherche les mises à jour automatiquement une fois par jour. Un badge \"NOUVEAU\" apparaîtra sur l'onglet Mises à jour lorsqu'une nouvelle version sera disponible." },
                    new FaqItem { Question = "Comment personnaliser l'arrière-plan du widget?", Answer = "Dans l'onglet Widget, vous pouvez sélectionner une image, ajuster sa position et utiliser les options \"Taille réelle\" ou \"Fondre l'image\" pour un meilleur rendu." },
                    new FaqItem { Question = "Puis-je modifier la disposition du widget?", Answer = "Oui ! Appuyez longuement sur le widget dans l'aperçu de l'application pour ouvrir l'éditeur. Vous pouvez inverser les contrôles d'en-tête, la gauche/droite et le texte vague/joueurs." },
                    new FaqItem { Question = "À quoi sert le bouton \"Mettre à jour (Cloud)\" ?", Answer = "Il télécharge la dernière liste des IP/Ports des serveurs s'ils ont changé. Utilisez-le si les serveurs affichent 'Offline'." },
                    new FaqItem { Question = "Puis-je suivre mes propres serveurs ?", Answer = "Par défaut, le widget est lié à KF2MOD-EU. Les utilisateurs avancés peuvent modifier manuellement 'servers.json' dans les données." },
                    new FaqItem { Question = "Pourquoi je vois \"Timeout\" ou \"Offline\" ?", Answer = "Le serveur redémarre, votre FAI bloque le trafic UDP, ou l'optimisation de la batterie limite la tâche en arrière-plan." },
                    new FaqItem { Question = "Le widget draine-t-il ma batterie ?", Answer = "Très peu. Il se réveille à l'intervalle défini, récupère quelques kilo-octets et se rendort." },
                    new FaqItem { Question = "Comment effacer la liste des joueurs suivis ?", Answer = "Supprimez simplement les pseudos de la zone de texte dans les paramètres et cliquez sur 'Enregistrer'." },
                    new FaqItem { Question = "Puis-je utiliser un fond transparent ?", Answer = "Oui ! Réglez le curseur 'Alpha' sur 0 dans la palette de couleurs et assurez-vous qu'aucune image n'est sélectionnée." },
                    new FaqItem { Question = "Pourquoi les notifications sont-elles retardées ?", Answer = "Android restreint les tâches en arrière-plan pour économiser la batterie. Le widget se met à jour selon les limites du système." },
                    new FaqItem { Question = "Où puis-je signaler un bug ou suggérer une fonctionnalité ?", Answer = "Utilisez les boutons 'GitHub' ou 'Discord' de l'onglet 'À propos' pour contacter le développeur." }
                },
                "pl" => new List<FaqItem>
                {
                    new FaqItem { Question = "Jak dodać widżet?", Answer = "Przejdź do ekranu głównego, przytrzymaj puste miejsce, wybierz \"Widżety\" i znajdź KF2Monitor." },
                    new FaqItem { Question = "Dlaczego widżet się nie aktualizuje?", Answer = "Upewnij się, że optymalizacja baterii dla aplikacji jest wyłączona, a połączenie internetowe jest stabilne." },
                    new FaqItem { Question = "Jak śledzić graczy?", Answer = "Włącz śledzenie w ustawieniach i wprowadź dokładne pseudonimy graczy oddzielone przecinkami. Otrzymasz powiadomienia, gdy dołączą do serwera lub go opuszczą." },
                    new FaqItem { Question = "Jak działają powiadomienia o zaludnieniu serwera?", Answer = "Ustaw próg graczy. Otrzymasz powiadomienie, gdy serwer osiągnie tę liczbę. Uwaga: Działa to tylko dla serwerów włączonych w \"Widoczność serwerów\"." },
                    new FaqItem { Question = "Jak zaktualizować aplikację?", Answer = "Aplikacja automatycznie sprawdza dostępność aktualizacji raz dziennie. Gdy nowa wersja będzie dostępna, w zakładce Aktualizacje pojawi się plakietka \"NOWE\"." },
                    new FaqItem { Question = "Jak dostosować tło widżetu?", Answer = "W zakładce Widżet możesz wybrać obraz, dostosować jego pozycję i użyć opcji \"Pełnowymiarowy obraz\" lub \"Zanikanie obrazu\", aby uzyskać lepszy efekt." },
                    new FaqItem { Question = "Czy mogę zmienić układ widżetu?", Answer = "Tak! Przytrzymaj widżet w podglądzie, aby otworzyć Edytor. Możesz zamienić miejscami elementy nagłówka, lewą/prawą stronę oraz tekst fali/graczy." },
                    new FaqItem { Question = "Do czego służy przycisk \"Aktualizuj z chmury\"?", Answer = "Pobiera najnowszą listę IP/portów serwerów, jeśli uległy zmianie. Użyj go, jeśli serwery są w stanie 'Offline'." },
                    new FaqItem { Question = "Czy mogę śledzić własne serwery?", Answer = "Domyślnie widżet jest powiązany z KF2MOD-EU. Zaawansowani użytkownicy mogą ręcznie nadpisać 'servers.json' w danych aplikacji." },
                    new FaqItem { Question = "Dlaczego widzę \"Timeout\" lub \"Offline\"?", Answer = "Serwer może się restartować, dostawca blokuje UDP lub optymalizacja baterii ogranicza zadania w tle." },
                    new FaqItem { Question = "Czy widżet zużywa moją baterię?", Answer = "Bardzo mało. Budzi się w ustalonym interwale, pobiera kilka kilobajtów i wraca do uśpienia." },
                    new FaqItem { Question = "Jak wyczyścić listę śledzonych graczy?", Answer = "Po prostu usuń pseudonimy z pola tekstowego w ustawieniach i naciśnij 'Zapisz konfigurację'." },
                    new FaqItem { Question = "Czy mogę użyć przezroczystego tła?", Answer = "Tak! W próbniku kolorów ustaw suwak 'Alpha' na 0 i upewnij się, że nie wybrano obrazu tła." },
                    new FaqItem { Question = "Dlaczego powiadomienia są opóźnione?", Answer = "Android ogranicza dokładne zadania w tle, aby oszczędzać baterię. Widżet aktualizuje się tak często, jak pozwala system." },
                    new FaqItem { Question = "Gdzie mogę zgłosić błąd lub zasugerować nową funkcję?", Answer = "Użyj przycisków 'GitHub' lub 'Discord' w zakładce O aplikacji, aby skontaktować się z autorem." }
                },
                "de" => new List<FaqItem>
                {
                    new FaqItem { Question = "Wie füge ich das Widget hinzu?", Answer = "Gehen Sie zum Startbildschirm, drücken Sie lange auf eine leere Stelle, wählen Sie \"Widgets\" und suchen Sie KF2Monitor." },
                    new FaqItem { Question = "Warum wird das Widget nicht aktualisiert?", Answer = "Stellen Sie sicher, dass die Batterieoptimierung für die App deaktiviert ist und Ihre Internetverbindung stabil ist." },
                    new FaqItem { Question = "Wie verfolge ich Spieler?", Answer = "Aktivieren Sie die Verfolgung in den Einstellungen und geben Sie die genauen Spielernamen durch Kommata getrennt ein. Sie erhalten Benachrichtigungen, wenn diese einem Server beitreten oder ihn verlassen." },
                    new FaqItem { Question = "Wie funktionieren die Server-Populations-Benachrichtigungen?", Answer = "Legen Sie einen Spieler-Schwellenwert fest. Sie werden benachrichtigt, wenn ein Server diese Anzahl erreicht. Hinweis: Dies funktioniert nur für Server, die unter \"Server-Sichtbarkeit\" aktiviert sind." },
                    new FaqItem { Question = "Wie aktualisiere ich die App?", Answer = "Die App sucht einmal täglich automatisch nach Updates. Ein \"NEU\"-Badge erscheint auf der Update-Registerkarte, wenn eine neue Version verfügbar ist." },
                    new FaqItem { Question = "Wie passe ich den Widget-Hintergrund an?", Answer = "In der Widget-Registerkarte können Sie ein Bild auswählen, die Position anpassen und die Optionen \"Volle Größe\" oder \"Bild überblenden\" verwenden." },
                    new FaqItem { Question = "Kann ich das Widget-Layout ändern?", Answer = "Ja! Drücken Sie in der App-Vorschau lange auf das Widget, um den Editor zu öffnen. Sie können Kopfzeilenelemente, Links/Rechts und Welle/Spieler-Text vertauschen." },
                    new FaqItem { Question = "Wofür ist die Schaltfläche \"Aus der Cloud aktualisieren\"?", Answer = "Sie lädt die neueste IP/Port-Liste für Server herunter, falls diese sich geändert haben. Nutzen Sie dies, wenn Server als 'Offline' angezeigt werden." },
                    new FaqItem { Question = "Kann ich meine eigenen Server verfolgen?", Answer = "Standardmäßig ist das Widget an KF2MOD-EU gebunden. Erfahrene Benutzer können 'servers.json' manuell überschreiben." },
                    new FaqItem { Question = "Warum sehe ich \"Timeout\" oder \"Offline\"?", Answer = "Der Server startet möglicherweise neu, Ihr Provider blockiert UDP, oder die Batterieoptimierung schränkt Hintergrundaufgaben ein." },
                    new FaqItem { Question = "Entlädt das Widget meinen Akku?", Answer = "Sehr wenig. Es wacht nur im eingestellten Intervall auf, holt ein paar Kilobyte und geht wieder schlafen." },
                    new FaqItem { Question = "Wie lösche ich die Liste der verfolgten Spieler?", Answer = "Löschen Sie einfach die Namen aus dem Textfeld und drücken Sie auf 'Konfiguration speichern'." },
                    new FaqItem { Question = "Kann ich einen transparenten Hintergrund verwenden?", Answer = "Ja! Setzen Sie den 'Alpha'-Schieberegler in der Farbauswahl auf 0 und entfernen Sie das Hintergrundbild." },
                    new FaqItem { Question = "Warum verzögern sich Benachrichtigungen?", Answer = "Android beschränkt genaue Hintergrundaufgaben, um Akku zu sparen. Das Widget wird aktualisiert, sobald das System es zulässt." },
                    new FaqItem { Question = "Wo kann ich einen Fehler melden oder eine Funktion vorschlagen?", Answer = "Verwenden Sie die Schaltflächen 'GitHub' oder 'Discord' im Über-Tab, um uns zu kontaktieren." }
                },
                "ja" => new List<FaqItem>
                {
                    new FaqItem { Question = "ウィジェットを追加するには？", Answer = "ホーム画面の空きスペースを長押しし、「ウィジェット」を選択してKF2Monitorを見つけます。" },
                    new FaqItem { Question = "ウィジェットが更新されないのはなぜですか？", Answer = "アプリのバッテリー最適化が無効になっており、インターネット接続が安定していることを確認してください。" },
                    new FaqItem { Question = "プレーヤーを追跡するには？", Answer = "設定で追跡を有効にし、プレーヤーの正確なニックネームをカンマ区切りで入力します。サーバーに参加または退出した際に通知が届きます。" },
                    new FaqItem { Question = "サーバーの人数通知はどのように機能しますか？", Answer = "プレイヤー数のしきい値を設定します。サーバーがこの数に達すると通知されます。注意：「サーバーの表示」で有効になっているサーバーでのみ機能します。" },
                    new FaqItem { Question = "アプリをアップデートするには？", Answer = "アプリは1日に1回自動的に更新を確認します。新しいバージョンが利用可能な場合は、更新タブに「新着」バッジが表示されます。" },
                    new FaqItem { Question = "ウィジェットの背景をカスタマイズするには？", Answer = "ウィジェットタブで画像を選択し、位置を調整して、「フルサイズ画像」または「背景にフェード」オプションを使用して外観を良くすることができます。" },
                    new FaqItem { Question = "ウィジェットのレイアウトは変更できますか？", Answer = "はい！アプリのプレビューでウィジェットを長押しするとエディターが開きます。ヘッダーのコントロール、左右、ウェーブ/プレイヤーのテキストを入れ替えることができます。" },
                    new FaqItem { Question = "「クラウドから更新」ボタンは何のためですか？", Answer = "サーバーのIP/ポートが変更された場合、最新のリストをダウンロードします。サーバーが「Offline」と表示された場合に使用してください。" },
                    new FaqItem { Question = "独自のサーバーを追跡できますか？", Answer = "デフォルトではKF2MOD-EU専用です。上級ユーザーはアプリデータの「servers.json」を手動で上書きできます。" },
                    new FaqItem { Question = "なぜ「Timeout」や「Offline」と表示されるのですか？", Answer = "サーバーの再起動、プロバイダーによるUDPブロック、またはバッテリー最適化が原因です。" },
                    new FaqItem { Question = "バッテリーの消費は激しいですか？", Answer = "非常に少ないです。設定した間隔で起動し、数キロバイトのデータを取得して再びスリープ状態になります。" },
                    new FaqItem { Question = "追跡しているプレーヤーのリストをクリアするには？", Answer = "設定のテキストボックスからニックネームを削除し、「設定を保存」を押すだけです。" },
                    new FaqItem { Question = "透明な背景は使えますか？", Answer = "はい！カラーピッカーで「Alpha」スライダーを0にし、背景画像が選択されていないことを確認します。" },
                    new FaqItem { Question = "なぜ通知が遅れるのですか？", Answer = "Androidはバッテリーを節約するためにバックグラウンドタスクを制限します。ウィジェットはシステムが許す範囲で更新されます。" },
                    new FaqItem { Question = "バグの報告や機能の提案はどこでできますか？", Answer = "「概要」タブの「GitHub」または「Discord」ボタンを使用して、開発者やコミュニティに連絡してください。" }
                },
                "en" => new List<FaqItem>
                {
                    new FaqItem { Question = "How to add the widget?", Answer = "Go to your home screen, long press an empty space, select \"Widgets\", and find KF2Monitor." },
                    new FaqItem { Question = "Why is the widget not updating?", Answer = "Ensure battery optimization is disabled for the app and your internet connection is stable." },
                    new FaqItem { Question = "How to track players?", Answer = "Enable tracking in settings and enter exact player nicknames separated by commas. You will receive notifications when they join or leave a server." },
                    new FaqItem { Question = "How do server population notifications work?", Answer = "Set a player threshold in settings. You will be notified when a server reaches this count. Note: This only works for servers enabled in \"Server Visibility\"." },
                    new FaqItem { Question = "How to update the app?", Answer = "The app checks for updates automatically once a day. A \"NEW\" badge will appear on the Updates tab when a new version is available." },
                    new FaqItem { Question = "How to customize the widget background?", Answer = "In the Widget tab, you can select an image, adjust its position, and use \"Full-size image\" or \"Fade background\" options for better blending." },
                    new FaqItem { Question = "Can I change the widget layout?", Answer = "Yes! Long press the widget in the app preview to open the Editor. You can swap header controls, left/right elements, and wave/players text." },
                    new FaqItem { Question = "What is the \"Update from Cloud\" button for?", Answer = "It downloads the latest IP/Port list for servers if they have changed. Use it if servers show as 'Offline'." },
                    new FaqItem { Question = "Can I track my own custom servers?", Answer = "By default, the widget is strictly bound to KF2MOD-EU servers. Advanced users can manually override 'servers.json' in the app data." },
                    new FaqItem { Question = "Why do I see \"Timeout\" or \"Offline\"?", Answer = "The server might be restarting, your provider blocks UDP traffic, or battery optimization restricts background tasks." },
                    new FaqItem { Question = "Does the widget drain my battery?", Answer = "Very little. It wakes up at your set interval, fetches a few kilobytes of data, and goes back to sleep." },
                    new FaqItem { Question = "How to clear the tracked players list?", Answer = "Just delete the nicknames from the text box in the settings and press 'Save Configuration'." },
                    new FaqItem { Question = "Can I use a transparent background?", Answer = "Yes! In the color picker, set the 'Alpha' slider to 0 and ensure no background image is selected." },
                    new FaqItem { Question = "Why are notifications sometimes delayed?", Answer = "Android restricts exact background tasks to save battery. The widget updates as close to your interval as the system allows." },
                    new FaqItem { Question = "Where can I report a bug or suggest a feature?", Answer = "Use the 'GitHub' or 'Discord' buttons in the About tab to reach out to the developer or the community." }
                },
                _ => new List<FaqItem>
                {
                    new FaqItem { Question = "How to add the widget?", Answer = "Go to your home screen, long press an empty space, select \"Widgets\", and find KF2Monitor." },
                    new FaqItem { Question = "Why is the widget not updating?", Answer = "Ensure battery optimization is disabled for the app and your internet connection is stable." },
                    new FaqItem { Question = "How to track players?", Answer = "Enable tracking in settings and enter exact player nicknames separated by commas. You will receive notifications when they join or leave a server." },
                    new FaqItem { Question = "How do server population notifications work?", Answer = "Set a player threshold in settings. You will be notified when a server reaches this count. Note: This only works for servers enabled in \"Server Visibility\"." },
                    new FaqItem { Question = "How to update the app?", Answer = "The app checks for updates automatically once a day. A \"NEW\" badge will appear on the Updates tab when a new version is available." },
                    new FaqItem { Question = "How to customize the widget background?", Answer = "In the Widget tab, you can select an image, adjust its position, and use \"Full-size image\" or \"Fade background\" options for better blending." },
                    new FaqItem { Question = "Can I change the widget layout?", Answer = "Yes! Long press the widget in the app preview to open the Editor. You can swap header controls, left/right elements, and wave/players text." },
                    new FaqItem { Question = "What is the \"Update from Cloud\" button for?", Answer = "It downloads the latest IP/Port list for servers if they have changed. Use it if servers show as 'Offline'." },
                    new FaqItem { Question = "Can I track my own custom servers?", Answer = "By default, the widget is strictly bound to KF2MOD-EU servers. Advanced users can manually override 'servers.json' in the app data." },
                    new FaqItem { Question = "Why do I see \"Timeout\" or \"Offline\"?", Answer = "The server might be restarting, your provider blocks UDP traffic, or battery optimization restricts background tasks." },
                    new FaqItem { Question = "Does the widget drain my battery?", Answer = "Very little. It wakes up at your set interval, fetches a few kilobytes of data, and goes back to sleep." },
                    new FaqItem { Question = "How to clear the tracked players list?", Answer = "Just delete the nicknames from the text box in the settings and press 'Save Configuration'." },
                    new FaqItem { Question = "Can I use a transparent background?", Answer = "Yes! In the color picker, set the 'Alpha' slider to 0 and ensure no background image is selected." },
                    new FaqItem { Question = "Why are notifications sometimes delayed?", Answer = "Android restricts exact background tasks to save battery. The widget updates as close to your interval as the system allows." },
                    new FaqItem { Question = "Where can I report a bug or suggest a feature?", Answer = "Use the 'GitHub' or 'Discord' buttons in the About tab to reach out to the developer or the community." }
                }
            };
        }
    }
}