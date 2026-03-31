import { useState } from 'react'
import reactLogo from './assets/react.svg'
import viteLogo from './assets/vite.svg'
import heroImg from './assets/hero.png'

function App() {
  const [count, setCount] = useState(0)

  return (
    <div className="min-h-screen bg-gray-900 text-white flex flex-col items-center justify-center px-4">
      
      {/* HERO */}
      <section className="text-center space-y-6">
        <div className="relative flex items-center justify-center">
          <img src={heroImg} className="w-40 h-auto opacity-80" alt="" />
          <img src={reactLogo} className="w-16 absolute -left-10 animate-spin-slow" alt="React logo" />
          <img src={viteLogo} className="w-16 absolute -right-10" alt="Vite logo" />
        </div>

        <div>
          <h1 className="text-4xl font-bold">Get started</h1>
          <p className="text-gray-400 mt-2">
            Edit <code className="bg-gray-800 px-2 py-1 rounded">src/App.tsx</code> and save to test{" "}
            <code className="bg-gray-800 px-2 py-1 rounded">HMR</code>
          </p>
        </div>

        <button
          className="px-6 py-2 bg-blue-600 hover:bg-blue-700 rounded-xl transition shadow-lg"
          onClick={() => setCount((count) => count + 1)}
        >
          Count is {count}
        </button>
      </section>

      {/* DIVIDER */}
      <div className="w-full max-w-xl h-px bg-gray-700 my-10"></div>

      {/* NEXT STEPS */}
      <section className="grid md:grid-cols-2 gap-8 max-w-3xl w-full">
        
        {/* Docs */}
        <div className="bg-gray-800 p-6 rounded-2xl shadow-lg">
          <h2 className="text-xl font-semibold mb-2">Documentation</h2>
          <p className="text-gray-400 mb-4">Your questions, answered</p>

          <ul className="space-y-3">
            <li>
              <a href="https://vite.dev/" target="_blank" className="flex items-center gap-2 hover:text-blue-400">
                <img className="w-5" src={viteLogo} alt="" />
                Explore Vite
              </a>
            </li>
            <li>
              <a href="https://react.dev/" target="_blank" className="flex items-center gap-2 hover:text-blue-400">
                <img className="w-5" src={reactLogo} alt="" />
                Learn more
              </a>
            </li>
          </ul>
        </div>

        {/* Social */}
        <div className="bg-gray-800 p-6 rounded-2xl shadow-lg">
          <h2 className="text-xl font-semibold mb-2">Connect with us</h2>
          <p className="text-gray-400 mb-4">Join the Vite community</p>

          <ul className="space-y-3">
            <li>
              <a href="https://github.com/vitejs/vite" target="_blank" className="hover:text-blue-400">
                GitHub
              </a>
            </li>
            <li>
              <a href="https://chat.vite.dev/" target="_blank" className="hover:text-blue-400">
                Discord
              </a>
            </li>
            <li>
              <a href="https://x.com/vite_js" target="_blank" className="hover:text-blue-400">
                X.com
              </a>
            </li>
            <li>
              <a href="https://bsky.app/profile/vite.dev" target="_blank" className="hover:text-blue-400">
                Bluesky
              </a>
            </li>
          </ul>
        </div>
      </section>

      {/* SPACER */}
      <div className="h-20"></div>
    </div>
  )
}

export default App