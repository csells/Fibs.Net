export default class FibsClient {
  session = { status: "closed" };
  settings = {};
  who = [];
  watching = {};

  get redoublesValues() { return this._getValidRedoublesValues(); }
  get timezoneValues() { return this._getValidTimezoneValues(); }

  constructor() {
    let parseBool = s => s === "1" || s === "YES";
    let parseString = s => s;
    let parseBoardTurnColor = s => s === "-1" ? "X" : s === "1" ? "O" : null;
    let parseBoardDice = s => s.split(":").map(d => parseInt(d));
    let parseBoardColor = s => s === "-1" ? "X" : "O";

    this._settingsParsers = {
      allowpip: parseBool,
      autoboard: parseBool,
      autodouble: parseBool,
      automove: parseBool,
      away: parseBool,
      bell: parseBool,
      boardstyle: parseInt,
      crawford: parseBool,
      double: parseBool,
      experience: parseInt,
      greedy: parseBool,
      moreboards: parseBool,
      moves: parseBool,
      notify: parseBool,
      rating: parseFloat,
      ratings: parseBool,
      ready: parseBool,
      redoubles: parseString,
      report: parseBool,
      silent: parseBool,
      telnet: parseBool,
      timezone: parseString,
      wrap: parseBool,
    };

    this._boardParsers = {
      player1: parseString,
      player2: parseString,
      matchLength: parseInt,
      player1Score: parseInt,
      player2Score: parseInt,
      board: parseString,
      turnColor: parseBoardTurnColor,
      player1Dice: parseBoardDice,
      player2Dice: parseBoardDice,
      doublingCube: parseInt,
      player1MayDouble: parseBool,
      player2MayDouble: parseBool,
      wasDoubled: parseBool,
      player1Color: parseBoardColor,
      direction: parseInt,
      player1Home: parseInt,
      player2Home: parseInt,
      player1Bar: parseInt,
      player2Bar: parseInt,
      canMove: parseInt,
      redoubles: parseInt,
    };
  }

  login(user, pass) {
    if (this._socket) { throw new Error("logout first"); }
    this._socket = new WebSocket("ws://localhost:5000/fibs"); // TODO

    this._socket.onopen = e => {
      console.log("socket: logging in as " + user);
      this.session.user = user;
      this.session.status = "opened";
      this._send("login " + user + " " + pass);
    };

    this._socket.onmessage = e => {
      //console.log("socket: message= " + e.data);
      JSON.parse(e.data).forEach(message => {
        this._processSession(message);
        this._processSettings(message);
        this._processWho(message);
        this._processWatch(message);
      });
    };

    this._socket.onclose = e => {
      console.log("socket: closed");
      this._socket = undefined;
      this.session = { status: "closed" };
      this.settings = {};
    };

    this._socket.onerror = e => {
      console.log("socket: error");
      this.session.error = "error"; // TODO
    };
  }

  //#region session
  logout() {
    console.log("logout");
    this._send("bye");
  }

  _send(s) {
    if (!this._socket) { throw new Error("login first"); }
    console.log(`send: ${s}`);
    this._socket.send(s);
  }

  _processSession(message) {
    let parseTimestamp = s => new Date(parseInt(s) * 1000);

    switch (message.cookie) {
      case "CLIP_WELCOME":
        this.session.status = "authenticated";
        this.session.lastLogin = parseTimestamp(message.crumbs.lastLogin);
        this.session.lastHost = message.crumbs.lastHost;
        this.session.motd = [];
        break;

      case "FIBS_MOTD":
        // strip all of the ASCII box stuff from around the MOTD
        let line = message.crumbs.message.trim();
        if (line.startsWith("+")) { return; }
        if (line.startsWith("|")) { line = line.substr(1); }
        if (line.endsWith("|")) { line = line.substr(0, line.length - 1); }
        line = line.trim();
        if (line === "") { return; }
        this.session.motd.push(line);
    }
  }
  //#endregion

  //#region settings
  toggleSetting(name) {
    this._send(`toggle ${name}`);
  }

  setAway(message) {
    if (message) { this._send(`away ${message}`); }
    else { this._send("back"); }
  }

  setRedoubles(value) {
    this._send(`set redoubles ${value}`);
  }

  setTimezone(value) {
    this._send(`set timezone ${value}`);
  }

  setBoardstyle(value) {
    this._send(`set boardstyle ${value}`);
  }

  _processSettings(message) {
    switch (message.cookie) {
      case "CLIP_OWN_INFO":
        delete message.crumbs.name; // not a setting
        this._parsePropertyFromCrumbs("settings", this._settingsParsers, message.crumbs);
        this._ensureClientSettings();
        break;

      case "FIBS_SettingsChange":
        console.log(`setting: ${message.crumbs.name}= ${message.crumbs.value}`);
        let m = {};
        m[message.crumbs.name] = message.crumbs.value;
        this._parsePropertyFromCrumbs("settings", this._settingsParsers, m);
        break;
    }
  }

  // ensure the required values are set up for the client
  _ensureClientSettings() {
    if (!this.settings.autoboard) { this.toggleSetting("autoboard"); }
    if (this.settings.bell) { this.toggleSetting("bell"); }
    if (!this.settings.moreboards) { this.toggleSetting("moreboards"); }
    if (!this.settings.notify) { this.toggleSetting("notify"); }
    if (!this.settings.report) { this.toggleSetting("report"); }
    this.setBoardstyle(3); // we don't get this to check w/ the other settings (although we could pull it)
  }
  //#endregion

  //#region who
  _processWho(message) {
    switch (message.cookie) {
      case "CLIP_WHO_INFO":
        let parseOptional = s => s === "-" ? null : s;
        let parseBool = s => s === "1";
        let crumbs = message.crumbs;
        let person = {
          name: crumbs.name,
          opponent: parseOptional(crumbs.opponent),
          watching: parseOptional(crumbs.watching),
          ready: parseBool(crumbs.ready),
          away: parseBool(crumbs.away),
          rating: parseFloat(crumbs.rating),
          experience: parseInt(crumbs.experience),
          idle: parseInt(crumbs.experience),
          login: crumbs.login,
          hostName: crumbs.hostName,
          client: parseOptional(crumbs.client),
          email: parseOptional(crumbs.email),
        };

        //console.log(`who: ${person.name}`);
        let i = this.who.findIndex(w => w.name === person.name);
        if (i === -1) { this.who.push(person); }
        else { this.who.splice(i, 1, person); }
        break;

      case "CLIP_LOGOUT":
        let j = this.who.findIndex(w => w.name === message.crumbs.name);
        if (j !== -1) { this.who.splice(j, 1); }
        break;
    }
  }
  //#endregion

  //#region watch
  watch(name) {
    if (!name) { throw new Error("watch: need a name"); }
    this.watching = {};
    this._send(`look ${name}`); // get initial board immediately
    this._send(`watch ${name}`); // watch for board changes
  }

  unwatch() {
    this.watching = {};
    this._send("unwatch");
  }

  _parsePropertyFromCrumbs(property, parsers, crumbs) {
    let obj = {};
    Object.keys(crumbs).forEach(name => {
      let parse = parsers[name];
      if (!parse) { throw new Error(`no parser for ${name}`); }
      obj[name] = parse(crumbs[name]);
    });

    // trigger anyone looking for any change on the top-level property at all
    // as well as all of the sub-properties
    // TODO: Needed for VueJS?
    this[property] = null;
    this[property] = obj;
  }

  _processWatch(message) {
    switch (message.cookie) {
      case "FIBS_Board":
        this._parsePropertyFromCrumbs("watching", this._boardParsers, message.crumbs);
        break;
    }
  }
  //#endregion

  //#region valid settings values
  _getValidRedoublesValues() {
    let values = ["unlimited", "none"];
    for (let i = 1; i < 100; i++) { values.push(i.toString()); }
    return values;
  }

  _getValidTimezoneValues() {
    return [
      "Africa/Abidjan", "Africa/Accra", "Africa/Addis_Ababa", "Africa/Algiers",
      "Africa/Asmera", "Africa/Bamako", "Africa/Bangui", "Africa/Banjul", "Africa/Bissau",
      "Africa/Blantyre", "Africa/Brazzaville", "Africa/Bujumbura", "Africa/Cairo",
      "Africa/Casablanca", "Africa/Ceuta", "Africa/Conakry", "Africa/Dakar",
      "Africa/Dar_es_Salaam", "Africa/Djibouti", "Africa/Douala", "Africa/El_Aaiun",
      "Africa/Freetown", "Africa/Gaborone", "Africa/Harare", "Africa/Johannesburg",
      "Africa/Kampala", "Africa/Khartoum", "Africa/Kigali", "Africa/Kinshasa",
      "Africa/Lagos", "Africa/Libreville", "Africa/Lome", "Africa/Luanda",
      "Africa/Lubumbashi", "Africa/Lusaka", "Africa/Malabo", "Africa/Maputo",
      "Africa/Maseru", "Africa/Mbabane", "Africa/Mogadishu", "Africa/Monrovia",
      "Africa/Nairobi", "Africa/Ndjamena", "Africa/Niamey", "Africa/Nouakchott",
      "Africa/Ouagadougou", "Africa/Porto-Novo", "Africa/Sao_Tome", "Africa/Timbuktu",
      "Africa/Tripoli", "Africa/Tunis", "Africa/Windhoek", "America/Adak",
      "America/Anchorage", "America/Anguilla", "America/Antigua", "America/Aruba",
      "America/Asuncion", "America/Barbados", "America/Belize", "America/Bogota",
      "America/Boise", "America/Buenos_Aires", "America/Caracas", "America/Catamarca",
      "America/Cayenne", "America/Cayman", "America/Chicago", "America/Cordoba",
      "America/Costa_Rica", "America/Cuiaba", "America/Curacao", "America/Dawson",
      "America/Dawson_Creek", "America/Denver", "America/Detroit", "America/Dominica",
      "America/Edmonton", "America/El_Salvador", "America/Ensenada", "America/Fortaleza",
      "America/Glace_Bay", "America/Godthab", "America/Goose_Bay", "America/Grand_Turk",
      "America/Grenada", "America/Guadeloupe", "America/Guatemala", "America/Guayaquil",
      "America/Guyana", "America/Halifax", "America/Havana", "America/Indiana/Knox",
      "America/Indiana/Marengo", "America/Indiana/Vevay", "America/Indianapolis",
      "America/Inuvik", "America/Iqaluit", "America/Jamaica", "America/Jujuy",
      "America/Juneau", "America/La_Paz", "America/Lima", "America/Los_Angeles",
      "America/Louisville", "America/Maceio", "America/Managua", "America/Manaus",
      "America/Martinique", "America/Mazatlan", "America/Mendoza", "America/Menominee",
      "America/Mexico_City", "America/Miquelon", "America/Montevideo", "America/Montreal",
      "America/Montserrat", "America/Nassau", "America/New_York", "America/Nipigon",
      "America/Nome", "America/Noronha", "America/Panama", "America/Pangnirtung",
      "America/Paramaribo", "America/Phoenix", "America/Port-au-Prince",
      "America/Port_of_Spain", "America/Porto_Acre", "America/Puerto_Rico",
      "America/Rainy_River", "America/Rankin_Inlet", "America/Regina", "America/Rosario",
      "America/Santiago", "America/Santo_Domingo", "America/Sao_Paulo",
      "America/Scoresbysund", "America/Shiprock", "America/St_Johns", "America/St_Kitts",
      "America/St_Lucia", "America/St_Thomas", "America/St_Vincent",
      "America/Swift_Current", "America/Tegucigalpa", "America/Thule",
      "America/Thunder_Bay", "America/Tijuana", "America/Tortola", "America/Vancouver",
      "America/Whitehorse", "America/Winnipeg", "America/Yakutat", "America/Yellowknife",
      "Antarctica/Casey", "Antarctica/Davis", "Antarctica/DumontDUrville",
      "Antarctica/Mawson", "Antarctica/McMurdo", "Antarctica/Palmer",
      "Antarctica/South_Pole", "Arctic/Longyearbyen", "Asia/Aden", "Asia/Alma-Ata",
      "Asia/Amman", "Asia/Anadyr", "Asia/Aqtau", "Asia/Aqtobe", "Asia/Ashkhabad",
      "Asia/Baghdad", "Asia/Bahrain", "Asia/Baku", "Asia/Bangkok", "Asia/Beirut",
      "Asia/Bishkek", "Asia/Brunei", "Asia/Calcutta", "Asia/Chungking", "Asia/Colombo",
      "Asia/Dacca", "Asia/Damascus", "Asia/Dubai", "Asia/Dushanbe", "Asia/Gaza",
      "Asia/Harbin", "Asia/Hong_Kong", "Asia/Irkutsk", "Asia/Ishigaki", "Asia/Jakarta",
      "Asia/Jayapura", "Asia/Jerusalem", "Asia/Kabul", "Asia/Kamchatka", "Asia/Karachi",
      "Asia/Kashgar", "Asia/Katmandu", "Asia/Krasnoyarsk", "Asia/Kuala_Lumpur",
      "Asia/Kuching", "Asia/Kuwait", "Asia/Macao", "Asia/Magadan", "Asia/Manila",
      "Asia/Muscat", "Asia/Nicosia", "Asia/Novosibirsk", "Asia/Omsk", "Asia/Phnom_Penh",
      "Asia/Pyongyang", "Asia/Qatar", "Asia/Rangoon", "Asia/Riyadh", "Asia/Saigon",
      "Asia/Seoul", "Asia/Shanghai", "Asia/Singapore", "Asia/Taipei", "Asia/Tashkent",
      "Asia/Tbilisi", "Asia/Tehran", "Asia/Thimbu", "Asia/Tokyo", "Asia/Ujung_Pandang",
      "Asia/Ulan_Bator", "Asia/Urumqi", "Asia/Vientiane", "Asia/Vladivostok",
      "Asia/Yakutsk", "Asia/Yekaterinburg", "Asia/Yerevan", "Atlantic/Azores",
      "Atlantic/Bermuda", "Atlantic/Canary", "Atlantic/Cape_Verde", "Atlantic/Faeroe",
      "Atlantic/Jan_Mayen", "Atlantic/Madeira", "Atlantic/Reykjavik",
      "Atlantic/South_Georgia", "Atlantic/St_Helena", "Atlantic/Stanley",
      "Australia/Adelaide", "Australia/Brisbane", "Australia/Broken_Hill",
      "Australia/Darwin", "Australia/Hobart", "Australia/Lindeman", "Australia/Lord_Howe",
      "Australia/Melbourne", "Australia/Perth", "Australia/Sydney", "Europe/Amsterdam",
      "Europe/Andorra", "Europe/Athens", "Europe/Belfast", "Europe/Belgrade",
      "Europe/Berlin", "Europe/Bratislava", "Europe/Brussels", "Europe/Bucharest",
      "Europe/Budapest", "Europe/Chisinau", "Europe/Copenhagen", "Europe/Dublin",
      "Europe/Gibraltar", "Europe/Helsinki", "Europe/Istanbul", "Europe/Kaliningrad",
      "Europe/Kiev", "Europe/Lisbon", "Europe/Ljubljana", "Europe/London",
      "Europe/Luxembourg", "Europe/Madrid", "Europe/Malta", "Europe/Minsk", "Europe/Monaco",
      "Europe/Moscow", "Europe/Oslo", "Europe/Paris", "Europe/Prague", "Europe/Riga",
      "Europe/Rome", "Europe/Samara", "Europe/San_Marino", "Europe/Sarajevo",
      "Europe/Simferopol", "Europe/Skopje", "Europe/Sofia", "Europe/Stockholm",
      "Europe/Tallinn", "Europe/Tirane", "Europe/Vaduz", "Europe/Vatican", "Europe/Vienna",
      "Europe/Vilnius", "Europe/Warsaw", "Europe/Zagreb", "Europe/Zurich",
      "Indian/Antananarivo", "Indian/Chagos", "Indian/Christmas", "Indian/Cocos",
      "Indian/Comoro", "Indian/Kerguelen", "Indian/Mahe", "Indian/Maldives",
      "Indian/Mauritius", "Indian/Mayotte", "Indian/Reunion", "Pacific/Apia",
      "Pacific/Auckland", "Pacific/Chatham", "Pacific/Easter", "Pacific/Efate",
      "Pacific/Enderbury", "Pacific/Fakaofo", "Pacific/Fiji", "Pacific/Funafuti",
      "Pacific/Galapagos", "Pacific/Gambier", "Pacific/Guadalcanal", "Pacific/Guam",
      "Pacific/Honolulu", "Pacific/Johnston", "Pacific/Kiritimati", "Pacific/Kosrae",
      "Pacific/Kwajalein", "Pacific/Majuro", "Pacific/Marquesas", "Pacific/Midway",
      "Pacific/Nauru", "Pacific/Niue", "Pacific/Norfolk", "Pacific/Noumea",
      "Pacific/Pago_Pago", "Pacific/Palau", "Pacific/Pitcairn", "Pacific/Ponape",
      "Pacific/Port_Moresby", "Pacific/Rarotonga", "Pacific/Saipan", "Pacific/Tahiti",
      "Pacific/Tarawa", "Pacific/Tongatapu", "Pacific/Truk", "Pacific/Wake",
      "Pacific/Wallis", "Pacific/Yap",
    ];
  }
  //#endregion
}
