import React, { Component } from 'react';
import { BrowserRouter as Router, Route, Switch } from 'react-router-dom';
import { Navbar, Nav, NavItem } from 'react-bootstrap';
import { LinkContainer } from 'react-router-bootstrap';
import { createStore } from 'redux';
import { Provider } from 'react-redux';
import { LoggedInLinkContainer, LoggedOutLinkContainer, LoggedInRoute, Login, Register, Logout } from "./Auth";
import './App.css';
import Welcome from './Welcome';
import Watch from './Watch';

// reducer
function reducer(state, action) {
  switch (action.type) {
    case "LOGIN": return { user: action.user, password: action.password };
    case "REGISTER": return { user: action.user, password: action.password };
    case "LOGOUT": return { user: "", password: "" };
    default: return state;
  }
}

const store = createStore(reducer, {
  // default values
  user: "",
  password: "",
});

// TODO: log into FIBS
function loginA(user, password) { return { type: "LOGIN", user, password }; }

// TODO: register a new user with FIBS
function registerA(user, password) { return { type: "REGISTER", user, password }; }

//  TODO: log out of FIBS
function logoutA() { return { type: "LOGOUT" }; }

export default class App extends Component {
  login = (user, password, cb) => { store.dispatch(loginA(user, password)); cb(); }
  register = (user, password, cb) => { store.dispatch(registerA(user, password)); cb(); }
  logout = (cb) => { store.dispatch(logoutA()); cb(); }

  render() {
    const user = store.user;

    return (
      <Provider store={store}>
        <Router>
          <div>
            <Navbar collapseOnSelect fixedTop>
              <Navbar.Header>
                <Navbar.Brand>Play FIBS!</Navbar.Brand>
                <Navbar.Toggle />
              </Navbar.Header>
              <Navbar.Collapse>
                <Nav>
                  <LinkContainer exact to="/"><NavItem>Welcome</NavItem></LinkContainer>
                  <LoggedOutLinkContainer user={user} exact to="/login"><NavItem>Login</NavItem></LoggedOutLinkContainer>
                  <LoggedOutLinkContainer user={user} exact to="/register"><NavItem>Register</NavItem></LoggedOutLinkContainer>
                  <LoggedInLinkContainer user={user} exact to="/logout"><NavItem>Logout</NavItem></LoggedInLinkContainer>
                  <LoggedInLinkContainer user={user} exact to="/watch"><NavItem>Watch</NavItem></LoggedInLinkContainer>
                </Nav>
              </Navbar.Collapse>
            </Navbar>
            <Switch>
              <Route exact path="/" component={Welcome} />
              <Route exact path="/login" component={() => <Login login={this.login} />} />
              <Route exact path="/register" component={() => <Register register={this.register} />} />
              <LoggedInRoute user={user} exact path="/logout" component={() => <Logout logout={this.logout} />} />
              <LoggedInRoute user={user} exact path="/watch" component={() => <Watch />} />
              <Route render={() => <div>404</div>} />
            </Switch>
            <div>user: {user}</div>
          </div>
        </Router>
      </Provider>
    );
  }
}
