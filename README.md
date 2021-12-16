# qcs-whitelist
This is a basic .NET 6 program which will take a set of minecraft IGNs and an email address, compare it against a list of known members and then use PlayerDB to get the UUIDs for those players, then finally generates the appropiate JSON, writes it to a file, then instructs the minecraft server to reload the whitelist

You'll need to provide a credentials.json and spreadsheets.txt file: 
- The `credentials.json` file can be produced from the google cloud console (see any of the sheets api quickstart tutorials)
- The `spreadsheets.txt` file is just the spreadsheet IDs for the form and member list, on seperate lines, in that order
    e.g. for this sheet: https://docs.google.com/spreadsheets/d/YyYYx_xxxxx6xxxx0xxxxxZZZzzZZZ_yY-xXXXXxXXx, `YyYYx_xxxxx6xxxx0xxxxxZZZzzZZZ_yY-xXXXXxXXx` is the ID
