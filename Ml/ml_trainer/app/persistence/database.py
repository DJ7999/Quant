# app/database.py
from sqlalchemy import create_engine
from sqlalchemy.orm import sessionmaker
from contextlib import contextmanager

# This matches your Docker container's port (5433)
DATABASE_URL = "postgresql+psycopg://postgres:YourSecurePassword123!@127.0.0.1:5433/quant_db"

# Create the core engine that manages connections to Postgres
engine = create_engine(
    DATABASE_URL, 
    pool_size=10,         # Keeps up to 10 connections open for your worker
    max_overflow=20       # Allows scaling up if your ML training spikes
)

# This is our factory for creating brief database sessions
SessionLocal = sessionmaker(bind=engine, autoflush=False, autocommit=False)

@contextmanager
def get_db_session():
    """Context manager to automatically open, commit, and close connections safely."""
    session = SessionLocal()
    try:
        yield session
    except Exception:
        session.rollback()  # Rollback changes if your ML code crashes mid-transaction
        raise
    finally:
        session.close()     # Crucial! Safely closes the connection back to the pool