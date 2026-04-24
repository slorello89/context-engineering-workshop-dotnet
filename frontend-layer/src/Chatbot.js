import React, { useState, useRef, useEffect } from 'react';
import axios from 'axios';
import { FaPaperPlane } from 'react-icons/fa';
import './Chatbot.css';

const apiBaseUrl = process.env.REACT_APP_API_BASE_URL
    || (window.location.port === '3000' ? 'http://localhost:8081' : '');

const Chatbot = () => {
    const [messages, setMessages] = useState([]);
    const [input, setInput] = useState('');
    const [loading, setLoading] = useState(false);
    const [lastUserTimestamp, setLastUserTimestamp] = useState(null);
    const chatboxRef = useRef(null);

    useEffect(() => {
        if (chatboxRef.current) {
            chatboxRef.current.scrollTop = chatboxRef.current.scrollHeight;
        }
    }, [messages, loading]);

    const handleSend = async () => {
        if (input.trim() === '') return;
        const timestamp = Date.now();
        const newMessage = { text: input, sender: 'user', timestamp };
        setMessages([...messages, newMessage]);
        setInput('');
        setLoading(true);
        setLastUserTimestamp(timestamp);

        try {
            const response = await axios.get(`${apiBaseUrl}/ai/chat/string`, {
                params: { query: input }
            });
            const elapsed = Date.now() - timestamp;
            const aiMessage = { text: response.data, sender: 'ai', elapsed };
            setMessages(prevMessages => [...prevMessages, aiMessage]);
        } catch (error) {
            console.error("Error fetching AI response", error);
        } finally {
            setLoading(false);
        }
    };

    const handleInputChange = (e) => {
        setInput(e.target.value);
    };

    const handleKeyPress = (e) => {
        if (e.key === 'Enter') {
            handleSend();
        }
    };

    // Helper to format elapsed ms to seconds with 1 decimal
    const formatElapsed = (ms) => {
        if (!ms) return null;
        return (ms / 1000).toFixed(1) + 's';
    };

    return (
        <div className="chatbot-container">
            <div className="chat-header">
                <img src="ctx-eng-with-langchain4j.png" alt="Chatbot Logo" className="chat-logo" />
            </div>
            <div className="chatbox" ref={chatboxRef}>
                {messages.map((message, index) => (
                    <div key={index} className={`message-container ${message.sender}`}>
                        <img
                            src={message.sender === 'user' ? 'user-icon.png' : 'ai-assistant.png'}
                            alt={`${message.sender} avatar`}
                            className="avatar"
                        />
                        <div className={`message ${message.sender}`}>
                            {message.text}
                            {message.sender === 'ai' && message.elapsed && (
                                <span className="elapsed-time">
                                    {message.elapsed < 1000
                                        ? `(${message.elapsed} ms)`
                                        : `(${formatElapsed(message.elapsed)})`}
                                </span>
                            )}
                        </div>
                    </div>
                ))}
                {loading && (
                    <div className="message-container ai">
                        <img src="ai-assistant.png" alt="AI avatar" className="avatar" />
                        <div className="message ai">...</div>
                    </div>
                )}
            </div>
            <div className="input-container">
                <input
                    type="text"
                    value={input}
                    onChange={handleInputChange}
                    onKeyPress={handleKeyPress}
                    placeholder="Type your message..."
                />
                <button onClick={handleSend}>
                    <FaPaperPlane />
                </button>
            </div>
        </div>
    );
};

export default Chatbot;
