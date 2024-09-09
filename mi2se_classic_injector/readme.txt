Program służy do konwertowania pliku .po znajdującego się w targecie OmegaT do plików txt. Pliki txt następnie należy przekonwertować do plików .info za pomocą MISETranslatora.

INSTRUKCJA:
Należy ustawić tryb programu na ErrorFromPo oraz ustawić odpowiednie ścieżki do plików. Program pobiera linie z pliku po, a jeśli ich nie znajdzie to pobiera je z wskazanego pliku NewPolPath. Wymagana konfiguracja to:
MainSettings.ErrorPath - lokalizacja pliku .po
MainSettings.NewPolPath - lokalizacja pliku txt, wypełniającego luki. Najczęściej będzie to po prostu najbardziej aktualny speech_pl.txt lub uipl.txt.
OutputCatalog - katalog w którym powstanie nowo utworzony output.txt

POZOSTAŁE TRYBY
Program ma również inne tryby, które były niezbędne w pierwotnym tworzeniu skryptów. Nie będę ich opisywał szczegółowo bo są skomplikowane, nie działają w każdej sytuacji(czasem wymagają ręcznych poprawek) i nie są niezbędne w aktualnym stanie spolszczenia. Opiszę jednak je krótko, bo może ktoś będzie miał z nich pożytek. Tryby:
Default - służył do wyciągnięcia z klasycznej wersji gry polskich napisów wraz z polskimi czcionkami(których aż do tej pory nie było w wersji specjalnej) i przypisania ich do wersji specjalnej. Skutecznie odnalazł około 90% linii, wymagał jednak ręcznych poprawek.
ErrorToPo - służył do eksportu plików txt do plików .po, jako źródła danych dla OmegiT.
Diff - służył do porównania wersji polskiej przygotowanej przez poprzednich twórców i mojej wersji polskich. Dzięki niemu byłem w stanie stwierdzić jak wiele linii udało mi się poprawnie wgrać do gry i odnaleźć ewentualne błędy w eksporcie.