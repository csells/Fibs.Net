import React from 'react';
import ReactDOM from 'react-dom';
import Routes from './routes';
import registerServiceWorker from './registerServiceWorker';
import './index.css';
import 'bootstrap/dist/css/bootstrap.css';

ReactDOM.render(<Routes />, document.getElementById('root'));
registerServiceWorker();
