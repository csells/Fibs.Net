export default class Session {
  status = "closed";
  error = null;
  lastLogin = null;
  lastHost = null;
  login = function (user, pass) {
    this.status = "opened";
  };
  logout = function () {
    this.status = "closed";
  };
};
