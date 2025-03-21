import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import axios from 'axios';

function postLoginData(username, password, setError, navigate) {
  axios.post('https://localhost:7187/api/auth/login', {
    username,
    password
  }, {
    withCredentials: true
  })
  .then(() => {
    console.log('User logged in');
    navigate('/');
  })
  .catch((error) => {
    setError(error.response.data);
  });
}

const Login = () => {
  const [username, setUsername] = useState('');
  const [password, setPassword] = useState('');

  const [error, setError] = useState(null);

  const navigate = useNavigate();

  const handleSubmit = (e) => {
    e.preventDefault();
    postLoginData(username, password, setError, navigate);
  };

  return (
    <div className="login-container">
      <h2>Login</h2>
      <form onSubmit={handleSubmit}>
        <div className="form-group">
          <label htmlFor="username">Username:</label>
          <input
            type="text"
            id="username"
            value={username}
            onChange={(e) => setUsername(e.target.value)}
            required
          />
        </div>
        <div className="form-group">
          <label htmlFor="password">Password:</label>
          <input
            type="password"
            id="password"
            value={password}
            onChange={(e) => setPassword(e.target.value)}
            required
          />
        </div>
        <button type="submit">Login</button>
        {error && <p className="error">{error}</p>}
      </form>
    </div>
  );
};

export default Login;