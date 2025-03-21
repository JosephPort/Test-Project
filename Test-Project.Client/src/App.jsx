import './App.css'
import { Routes, Route, BrowserRouter as Router } from 'react-router-dom'
import Home from './pages/Home'
import Register from './pages/auth/Register'
import Login from './pages/auth/Login'
import NotFound from './pages/NotFound'
import Navbar from './components/Navbar'

function App() {
  return (
    <Router>
      <Navbar />
      <Routes>
        <Route path="/" element={<Home />} />
        <Route path="auth">
          <Route path="register" element={<Register />} />
          <Route path="login" element={<Login />} />
        </Route>
        <Route path="*" element={<NotFound />} />
      </Routes>
    </Router>
  )
}

export default App
