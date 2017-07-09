import React from 'react';
import { BrowserRouter as Router, Route, Switch, Redirect, withRouter } from 'react-router-dom'
import { Navbar, Nav, NavItem } from 'react-bootstrap'
import { LinkContainer } from 'react-router-bootstrap'

const Welcome = () => <div>welcome</div>;

const Login = withRouter(({props, history}) => <button onClick={()=>{
    props.login("user", "pw");
    props.history.push(props.state.from || "/logout");
  }
}>login</button>);

const Register = withRouter(({props, history}) => <button onClick={()=>{
      props.register("user", "pw", props.history);
      props.history.push(props.state.from || "/logout");
    }
}>register</button>);

const Logout = withRouter(({props, history}) => <button onClick={()=>{
    props.logout(props.history);
    props.history.push(props.state.from || "/login");
  }
}>logout</button>);

const Watch = () => <div>watch</div>;
const Play = () => <div>play</div>;
const Chat = () => <div>chat</div>;

class Routes extends React.Component {
  state = {
    user: null,
  };

  constantRouteLinks = [
    { path: "/", text: "Welcome", component: Welcome },
  ];

  loggedOutRouteLinks = [
    { path: "/login", text: "Login", component: () => <Login login={this.login} /> },
    { path: "/register", text: "Register", component: () => <Register register={this.register} /> },
  ];

  loggedInRouteLinks = [
    { path: "/logout", text: "Logout", component: () => <Logout logout={this.logout} /> },
    { path: "/watch", text: "Watch", component: Watch },
    { path: "/play", text: "Play", component: Play },
    { path: "/chat", text: "Chat", component: Chat },
  ];

  login = (user, pw) => {
    this.setState({user: user});
  };

  register = (user, pw) => {
    this.setState({user: user});
    history.push(props.location.state.from || "/logout");
  };

  logout = () => {
    this.setState({user: null});
    history.push(props.location.state.from || "/login");
  };

  render() {
    let loggedIn = !!this.state.user;
    let routeLinks = constantRouteLinks.slice().concat(loggedIn ? loggedInRouteLinks : loggedOutRouteLinks);

    return (
      <Router>
        <div>
          <Navbar collapseOnSelect>
            <Navbar.Header>
              <Navbar.Brand>Play FIBS!</Navbar.Brand>
              <Navbar.Toggle />
            </Navbar.Header>
            <Navbar.Collapse>
              <Nav>
                {routeLinks.map(rl => (<LinkContainer key={rl.path} to={rl.path}><NavItem>{rl.text}</NavItem></LinkContainer>))}
              </Nav>
            </Navbar.Collapse>
          </Navbar>
          <Switch>
            {this.constantRouteLinks.map(rl => (<Route exact key={rl.path} path={rl.path} component={rl.component} />))}
            {this.loggedOutRouteLinks // login, register, ...
              .map(rl => !loggedIn
                ? (<Route key={rl.path} path={rl.path} component={rl.component} />)
                : (<Redirect key={rl.path} from={rl.path} to={{pathname: "/login", state: {from: this.props.location}}} />)
            )}
            {this.loggedInRouteLinks // play, watch, chat
              .map(rl => loggedIn
                ? (<Route key={rl.path} path={rl.path} component={rl.component} />)
                : (<Redirect key={rl.path} from={rl.path} to={{pathname: "/logout", state: {from: this.props.location}}} />)
            )}
            <Route component={NotFound} />
          </Switch>
          <div>user: {this.state.user}</div>
        </div>
      </Router>
    );
  }
};

export default Routes;